using System;
using System.Threading;
using System.Threading.Tasks;
using Itminus.Tags.OpcUaClient;
using Itminus.Tags.OpcUaClient.DirectTags;
using Itminus.Tags.Tests.Fakes;
using Xunit;

namespace Itminus.Tags.Tests.OpcUaClientTags;

public class OpcUaClientTagBuilderTests
{
    private static OpcUaClientTagBuilder CreateBuilder(ITagChannel? selfChannel, TagDescriptor? descriptor = null)
    {
        var grp = new TagGrp(new TagGrpDescriptor { Name = "g" }, new MockOpcUaChannel("mockChannel"));
        var builder = new OpcUaClientTagBuilder();
        builder
            .WithTagDescriptor(descriptor ?? new TagDescriptor { TagName = "t", RawAddress = "ns=1;s=Var1", TagKind = BuiltinTagKinds.FLOAT, TagSize = 4 })
            .WithParent(grp)
            .WithChannel(selfChannel);
        return builder;
    }

    [Fact]
    public void Build_WhenSelfChannelIsOpcUa_CreatesTagWithChannel()
    {
        var channel = new MockOpcUaChannel("opc");
        var builder = CreateBuilder(selfChannel: channel);

        var tag = builder.Build(channel);

        Assert.IsType<OpcUaClientDirectTag>(tag);
        Assert.Same(channel, tag.Channel);
    }

    [Fact]
    public void Build_WhenSelfChannelNotOpcUa_Throws()
    {
        var fakeChannel = new FakeSimpleChannel();
        var builder = CreateBuilder(selfChannel: fakeChannel);

        var ex = Assert.Throws<Exception>(() => builder.Build(null!));
        Assert.Contains(nameof(OpcUaClientTagChannel), ex.Message);
        Assert.Contains("t", ex.Message);
    }

    [Fact]
    public void Build_SetsTagNameFromDescriptor()
    {
        var descriptor = new TagDescriptor { 
            TagName = "myVar", 
            RawAddress = "ns=1;s=MyVar", 
            TagKind = BuiltinTagKinds.INT32, 
            TagSize = 4 
        };
        var builder = CreateBuilder(selfChannel: null, descriptor: descriptor);

        var tag = builder.Build(null!);

        Assert.Equal("myVar", tag.TagName());
    }

    [Fact]
    public void Build_SetsNormalizedAddressFromDescriptor()
    {
        var descriptor = new TagDescriptor { TagName = "v", RawAddress = "ns=1;s=Addr1", TagKind = BuiltinTagKinds.FLOAT, TagSize = 4 };
        var builder = CreateBuilder(selfChannel: null, descriptor: descriptor);

        var tag = builder.Build(null!);

        Assert.Equal("ns=1;s=Addr1", tag.NormalizedAddress());
    }

    private class FakeSimpleChannel : ITagChannel
    {
        public string ChannelName => "NotOpcUa";
        public string Driver => "FAKE";
        public Task EnsureConnectedAsync(bool force, CancellationToken ct) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;
        public void Dispose() { }
    }
}
