using System;
using System.Threading;
using System.Threading.Tasks;
using Itminus.Tags.SimpleFiles;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Itminus.Tags.Tests.SimpleFilesTags;

public class DirectTagBuilderTests
{
    private static readonly TagDescriptor DefaultDescriptor = new()
    {
        TagName = "t",
        RawAddress = "test.txt",
        TagKind = BuiltinTagKinds.INT32,
    };

    private static SimpleFilesDirectTagBuilder CreateBuilder(ITagChannel? selfChannel, TagDescriptor? descriptor = null)
    {
        var chdescriptor = new SimpleFilesTagChannelDescriptor()
        {
            Name = "ch", 
            BaseDir = "",
        };
        var channel = new SimpleFilesTagChannel(chdescriptor, NullLogger<SimpleFilesTagChannel>.Instance);
        var grp = new TagGrp(new TagGrpDescriptor { Name = "g" }, channel);
        var builder = new SimpleFilesDirectTagBuilder();
        builder
            .WithTagDescriptor(descriptor ?? DefaultDescriptor)
            .WithParent(grp)
            .WithChannel(selfChannel);
        return builder;
    }

    [Fact]
    public void Build_WhenSelfChannelIsNull_UsesFallbackChannel()
    {
        var builder = CreateBuilder(selfChannel: null);

        var tag = builder.Build(null!);

        Assert.IsType<IntDirectTag>(tag);
        Assert.Equal("t", tag.TagName());
    }

    [Fact]
    public void Build_WhenSelfChannelIsSimpleFiles_CreatesTagWithSelfChannel()
    {
        var descriptor = new SimpleFilesTagChannelDescriptor()
        {
            Name = "self-ch", 
            BaseDir = "",
        };
        var sfChannel = new SimpleFilesTagChannel(descriptor, NullLogger<SimpleFilesTagChannel>.Instance);
        var builder = CreateBuilder(selfChannel: sfChannel);

        var tag = builder.Build(null!);

        Assert.IsType<IntDirectTag>(tag);
        Assert.Equal("t", tag.TagName());
    }

    [Fact]
    public void Build_WhenSelfChannelNotSimpleFiles_Throws()
    {
        var fakeChannel = new FakeNonSimpleFilesChannel();
        var builder = CreateBuilder(selfChannel: fakeChannel);

        var ex = Assert.Throws<Exception>(() => builder.Build(null!));
        Assert.Contains(nameof(SimpleFilesTagChannel), ex.Message);
        Assert.Contains("t", ex.Message);
    }

    [Fact]
    public void Build_SetsTagNameFromDescriptor()
    {
        var descriptor = new TagDescriptor
        {
            TagName = "myTag",
            RawAddress = "data.txt",
            TagKind = BuiltinTagKinds.FLOAT,
        };
        var builder = CreateBuilder(selfChannel: null, descriptor: descriptor);

        var tag = builder.Build(null!);

        Assert.Equal("myTag", tag.TagName());
    }

    [Fact]
    public void Build_SetsNormalizedAddressFromDescriptor()
    {
        var descriptor = new TagDescriptor
        {
            TagName = "v",
            RawAddress = "path/to/file.txt",
            TagKind = BuiltinTagKinds.FLOAT,
        };
        var builder = CreateBuilder(selfChannel: null, descriptor: descriptor);

        var tag = builder.Build(null!);

        Assert.Equal("path/to/file.txt", tag.NormalizedAddress());
    }

    private class FakeNonSimpleFilesChannel : ITagChannel
    {
        public TagChannelDescriptor Descriptor => new TagChannelDescriptor
        {
            Name = "NotSimpleFiles",
            Driver = "Fake",
        };
        public Task EnsureConnectedAsync(bool force, CancellationToken ct) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;
        public void Dispose() { }
    }
}
