using System;
using System.Threading;
using System.Threading.Tasks;
using Itminus.Tags.OpcUaClient;
using Itminus.Tags.OpcUaClient.Cbnts;
using Xunit;

namespace Itminus.Tags.Tests.OpcUaClientTags;


public class OpcUaClientTagCbntTests
{
    [Fact]
    public void Constructor_SetsMetadata()
    {
        var descriptor = new TagCbntDescriptor
        {
            Name = "myCbnt",
            StartAddress = "ns=1",
            IsEnabled = true,
        };

        var cbnt = new OpcUaClientTagCbnt(descriptor);

        Assert.Equal("myCbnt", cbnt.TagName());
        Assert.Equal("ns=1", cbnt.StartAddress);
        Assert.True(cbnt.IsEnabled);
        Assert.Empty(cbnt.Children);
    }

    [Fact]
    public void CacheSize_EqualsBagCount()
    {
        var cbnt = new OpcUaClientTagCbnt(new TagCbntDescriptor { Name = "c", StartAddress = "ns=1" });

        Assert.Equal(0, cbnt.CacheSize);

        cbnt.Bag.TryAdd(new Opc.Ua.NodeId("test", 1), new Opc.Ua.DataValue());
        Assert.Equal(1, cbnt.CacheSize);
    }

    [Fact]
    public void ResizeCache_DoesNothing()
    {
        var cbnt = new OpcUaClientTagCbnt(new TagCbntDescriptor { Name = "c", StartAddress = "ns=1" });

        // ResizeCache 在 OpcUa 实现中不做任何事
        cbnt.ResizeCache(999);
        Assert.Equal(0, cbnt.CacheSize);
    }

    [Fact]
    public async Task ReadAsync_WhenChannelNotOpcUa_Throws()
    {
        var cbnt = new OpcUaClientTagCbnt(new TagCbntDescriptor { Name = "c", StartAddress = "ns=1" })
        {
            Channel = new FakeSimpleChannel(),
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => cbnt.ReadAsync(CancellationToken.None));
        Assert.Contains(nameof(OpcUaClientTagChannel), ex.Message);
    }

    [Fact]
    public async Task WriteAsync_WhenChannelNotOpcUa_Throws()
    {
        var cbnt = new OpcUaClientTagCbnt(new TagCbntDescriptor { Name = "c", StartAddress = "ns=1" })
        {
            Channel = new FakeSimpleChannel(),
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => cbnt.WriteAsync(CancellationToken.None));
        Assert.Contains(nameof(OpcUaClientTagChannel), ex.Message);
    }

    /// <summary>
    /// 模拟一个 ITagCbntor，但不是 OpcUaClientTagCbntor
    /// </summary>
    private class FakeTagCbntor : ITagCbntor
    {
        public TagDescriptor TagDescriptor { get; set; } = new();
        public object? Value { get; set; }
        public DateTime Timestamp { get; set; }
        public event TagSyncEventHandler? OnTagRead { add { } remove { } }
        public event TagSyncEventHandler? OnTagWritten { add { } remove { } }
        public int TagOffset { get; set; }
        public int CacheOffset { get; set; }
        public bool IsDirty { get; set; }
        public bool IsScaned { get; set; }
        public ITagChannel? Channel => null;
        public TagContainer? Parent { get; set; }

        public FakeTagCbntor(ITagCbnt tagCbnt) { TagCbnt = tagCbnt; }
        public ITagCbnt TagCbnt { get; set; }

        public void NotifyTagRead() { }
        public void NotifyTagWritten() { }
        public Task ReadAsync(CancellationToken ct) => Task.CompletedTask;
        public Task WriteAsync(CancellationToken ct) => Task.CompletedTask;
    }

    private class FakeSimpleChannel : ITagChannel
    {
        public string ChannelName => "Fake";
        public string Driver => "FAKE";
        public Task EnsureConnectedAsync(bool force, CancellationToken ct) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;
        public void Dispose() { }
    }
}
