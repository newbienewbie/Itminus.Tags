using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Itminus.Tags.McpServer;
using Itminus.Tags.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Itminus.Tags.Tests.McpServerTests;

public class TagsMcpServerToolsTests : IAsyncDisposable
{
    private readonly ServiceProvider _root;
    private readonly IServiceScope _scope;
    private readonly IServiceProvider _sp;

    public TagsMcpServerToolsTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTagsProjectServices(b =>
        {
            b.AddFakedSupport();
            b.AddFakedTagSupport();
        });
        _root = services.BuildServiceProvider();
        _scope = _root.CreateScope();
        _sp = _scope.ServiceProvider;
    }

    public ValueTask DisposeAsync()
    {
        _scope.Dispose();
        _root.Dispose();
        return default;
    }

    private ITagsProjectFactory Factory => _sp.GetRequiredService<ITagsProjectFactory>();
    private static ILogger<TagsMcpServerTools> Logger => NullLogger<TagsMcpServerTools>.Instance;

    private static XElement TestXml => new("root",
        new XElement("Channel", new XAttribute("name", "fake"), new XAttribute("driver", "fake")),
        new XElement("TagGrp",
            new XAttribute("name", "g1"), new XAttribute("isEntry", "true"),
            new XAttribute("isEnabled", "true"), new XAttribute("channel", "fake"),
            new XElement("TagGrp", new XAttribute("name", "sub"),
                new XElement("Tag", new XAttribute("name", "bit_tag"), new XAttribute("type", "BIT"), new XAttribute("access", "RW"), new XAttribute("address", "0")),
                new XElement("Tag", new XAttribute("name", "int16_tag"), new XAttribute("type", "INT16"), new XAttribute("access", "RW"), new XAttribute("address", "2")),
                new XElement("Tag", new XAttribute("name", "ro_tag"), new XAttribute("type", "BIT"), new XAttribute("access", "RO"), new XAttribute("address", "3"))
            )
        ),
        new XElement("TagGrp",
            new XAttribute("name", "g2"), new XAttribute("isEntry", "true"),
            new XAttribute("isEnabled", "true"), new XAttribute("channel", "fake"),
            new XElement("Tag", new XAttribute("name", "float_tag"), new XAttribute("type", "FLOAT"), new XAttribute("access", "RW"), new XAttribute("address", "0")),
            new XElement("Tag", new XAttribute("name", "int32_tag"), new XAttribute("type", "INT32"), new XAttribute("access", "RW"), new XAttribute("address", "4"))
        )
    );

    private TagsMcpServerTools MakeTools(ITagsProject proj) =>
        new(new ProjectCtrl(proj), Logger);

    private static void AssertSuccess(WriteResult result)
    {
        Assert.True(result.Success, $"Expected success, got: {result.Message}");
        Assert.NotEmpty(result.Message);
    }

    private static void AssertFail(WriteResult result, string? expectedFragment = null)
    {
        Assert.False(result.Success, $"Expected failure, got success: {result.Message}");
        Assert.NotEmpty(result.Message);
        if (expectedFragment is not null)
            Assert.Contains(expectedFragment, result.Message);
    }


    [Fact]
    public void EnsureProject_WhenNoProject_Throws()
    {
        var tools = new TagsMcpServerTools(new ProjectCtrl(null), Logger);
        Assert.Throws<InvalidOperationException>(() => tools.ReadTagValue("g1/sub/bit_tag"));
    }


    #region 测试 读取
    [Fact]
    public void ReadTag_ExistingTag_ReturnsValue()
    {
        using var proj = Factory.Create(null!, TestXml);
        var tools = MakeTools(proj);
        var result = tools.ReadTagValue("g1/sub/bit_tag");
        Assert.NotNull(result);
        Assert.NotNull(result.Descriptor);
        Assert.Equal("bit_tag", result.Descriptor.TagName);
        Assert.Equal(BuiltinTagKinds.BIT, result.Descriptor.TagKind);
        Assert.Equal(TagAccessMode.RW, result.Descriptor.AccessMode);
    }

    [Fact]
    public void ReadTag_NonExistentPath_Throws()
    {
        using var proj = Factory.Create(null!, TestXml);
        var tools = MakeTools(proj);
        Assert.Throws<InvalidOperationException>(() => tools.ReadTagValue("g1/nonexistent/tag"));
    }


    [Fact]
    public void ReadTags_MultipleTags_ReturnsAll()
    {
        using var proj = Factory.Create(null!, TestXml);
        var tools = MakeTools(proj);
        var results = tools.ReadTagValues(new[] { "g1/sub/bit_tag", "g1/sub/int16_tag", "g2/int32_tag" });
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.NotNull(r.Descriptor));
        Assert.All(results, r => Assert.Null(r.Error));
    }

    [Fact]
    public void ReadTags_SomeNonExistent_ReturnsErrorsForThose()
    {
        using var proj = Factory.Create(null!, TestXml);
        var tools = MakeTools(proj);
        var results = tools.ReadTagValues(new[] { "g1/sub/bit_tag", "g1/no/such/tag" });
        Assert.Equal(2, results.Count);
        Assert.Null(results[0].Error);
        Assert.NotNull(results[1].Error);
        Assert.Null(results[1].Descriptor);
    }
    #endregion


    #region 测试 写入
    [Fact]
    public async Task WriteTag_RwTag_SetsValue()
    {
        using var proj = Factory.Create(null!, TestXml);
        using var cts = new CancellationTokenSource();
        _ = Task.Run(() => proj.RunAsync(cts.Token));
        var tools = MakeTools(proj);

        var result = await tools.WriteTagValue("g1/sub/bit_tag", "true", waitForCompletion: true);
        AssertSuccess(result);

        cts.Cancel();
    }

    [Fact]
    public async Task WriteTag_ReadOnly_ReturnsError()
    {
        using var proj = Factory.Create(null!, TestXml);
        using var cts = new CancellationTokenSource();
        _ = Task.Run(() => proj.RunAsync(cts.Token));
        var tools = MakeTools(proj);

        var result = await tools.WriteTagValue("g1/sub/ro_tag", "true");
        AssertFail(result, "read-only");

        cts.Cancel();
    }

    [Fact]
    public async Task WriteTag_NonExistent_ReturnsError()
    {
        using var proj = Factory.Create(null!, TestXml);
        using var cts = new CancellationTokenSource();
        _ = Task.Run(() => proj.RunAsync(cts.Token));
        var tools = MakeTools(proj);

        var result = await tools.WriteTagValue("g1/no/such/tag", "42");
        AssertFail(result, "Failed to find tag");

        cts.Cancel();
    }


    [Fact]
    public async Task WriteTags_BatchAcrossEntries_GroupsCorrectly()
    {
        using var proj = Factory.Create(null!, TestXml);
        using var cts = new CancellationTokenSource();
        _ = Task.Run(() => proj.RunAsync(cts.Token));
        var tools = MakeTools(proj);

        var result = await tools.WriteTagValues(new Dictionary<string, string>
        {
            ["g1/sub/bit_tag"] = "true",
            ["g1/sub/int16_tag"] = "123",
            ["g2/float_tag"] = "3.14",
            ["g2/int32_tag"] = "42",
        }, waitForCompletion: true);

        AssertSuccess(result);
        Assert.Contains("4 tag(s) across 2 entry group(s)", result.Message);

        cts.Cancel();
    }

    [Fact]
    public async Task WriteTags_IncludesReadOnly_Aborts()
    {
        using var proj = Factory.Create(null!, TestXml);
        using var cts = new CancellationTokenSource();
        _ = Task.Run(() => proj.RunAsync(cts.Token));
        var tools = MakeTools(proj);

        var result = await tools.WriteTagValues(new Dictionary<string, string>
        {
            ["g1/sub/bit_tag"] = "true",
            ["g1/sub/ro_tag"] = "true",
        });
        AssertFail(result, "read-only");

        cts.Cancel();
    }

    [Fact]
    public async Task WriteTags_NonExistent_ReturnsError()
    {
        using var proj = Factory.Create(null!, TestXml);
        using var cts = new CancellationTokenSource();
        _ = Task.Run(() => proj.RunAsync(cts.Token));
        var tools = MakeTools(proj);

        var result = await tools.WriteTagValues(new Dictionary<string, string>
        {
            ["g1/sub/bit_tag"] = "true",
            ["g1/no/such"] = "0",
        });
        AssertFail(result, "Failed to find tag");

        cts.Cancel();
    }


    [Fact]
    public async Task WriteTag_ConvertsInt16Value()
    {
        using var proj = Factory.Create(null!, TestXml);
        using var cts = new CancellationTokenSource();
        _ = Task.Run(() => proj.RunAsync(cts.Token));
        var tools = MakeTools(proj);

        var result = await tools.WriteTagValue("g1/sub/int16_tag", "42");
        AssertSuccess(result);

        cts.Cancel();
    }

    [Fact]
    public async Task WriteTag_ConvertsFloatValue()
    {
        using var proj = Factory.Create(null!, TestXml);
        using var cts = new CancellationTokenSource();
        _ = Task.Run(() => proj.RunAsync(cts.Token));
        var tools = MakeTools(proj);

        var result = await tools.WriteTagValue("g2/float_tag", "3.14");
        AssertSuccess(result);

        cts.Cancel();
    }
    #endregion


    #region 结果测试
    [Fact]
    public void WriteResult_Ok_IsSuccess()
    {
        var wr = WriteResult.Ok("done");
        Assert.True(wr.Success);
        Assert.Equal("done", wr.Message);
    }

    [Fact]
    public void WriteResult_Fail_IsNotSuccess()
    {
        var wr = WriteResult.Fail("bad");
        Assert.False(wr.Success);
        Assert.Equal("bad", wr.Message);
    }


    [Fact]
    public void TagValue_WithError_HasNullDescriptor()
    {
        var tv = new TagValue(null, null, DateTime.MinValue, "boom");
        Assert.Null(tv.Descriptor);
        Assert.Null(tv.Value);
        Assert.Equal("boom", tv.Error);
    }

    [Fact]
    public void TagValue_Success_HasDescriptorAndValue()
    {
        var desc = new TagDescriptor { TagName = "t", TagKind = BuiltinTagKinds.BIT, AccessMode = TagAccessMode.RW };
        var now = DateTime.UtcNow;
        var tv = new TagValue(desc, true, now);
        Assert.Same(desc, tv.Descriptor);
        Assert.Equal(true, tv.Value);
        Assert.Equal(now, tv.Timestamp);
        Assert.Null(tv.Error);
    }
    #endregion

    private sealed class ProjectCtrl(ITagsProject? project) : ITagsProjectCtrl
    {
        public ITagsProject? Project => project;
        public Func<Exception, Task<bool>>? OnStartingException { get; set; }
        public event TagsProjectStartedOrStopped? StartedOrStopped;
        public Task StartPollAsync(string? dir, XElement? root, Func<ITagsProject, CancellationToken, Task> hook)
            => throw new NotSupportedException();
        public Task StopAsync() => Task.CompletedTask;
    }
}
