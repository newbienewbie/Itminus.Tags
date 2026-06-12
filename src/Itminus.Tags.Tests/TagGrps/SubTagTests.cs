using Itminus.Tags.S7;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Itminus.Tags.Tests.TagGrps;

public class SubTagTests
{

    class NoChannelTag : Tag<byte>
    {
        public NoChannelTag(TagDescriptor descriptor) : base(descriptor)
        {
            // 有意让这个Tag没有通道，测试冒泡式访问通道
            this.Channel = null!;
        }

        public override ITagChannel? Channel { get; set; } 

        public override Task ReadAsync(CancellationToken ct) => Task.CompletedTask;

        public override Task WriteAsync(CancellationToken ct) => Task.CompletedTask;
    }

    [Fact]
    public void Test()
    {
        var channelFactory = new S7TagChannelFactory(new LoggerFactory());
        var channel = channelFactory.Create(new TagChannelDescriptor()
        {
            Driver = "S7",
            Name = "S7-1",
            Extras = new Dictionary<string, XElement>() { }
        });
        var loggerFactory = new LoggerFactory();

        var cbnt = new S7TagCbntBuilder("cbnt1", "DB200.100.1")
            .Configure(builder =>
            {
                var tagFactory = builder.MakeS7TagFactory();

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-请求-标志",
                    RawAddress = "DB200.100.1",
                    TagKind = BuiltinTagKinds.BIT,
                    TagSize = 1,
                }));

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-请求-料号",
                    RawAddress = "DB200.102",
                    TagKind = BuiltinTagKinds.BYTE,
                    TagSize = 1,
                }));

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-请求-程序号",
                    RawAddress = "DB200.104",
                    TagKind = BuiltinTagKinds.INT16,
                    TagSize = 2,
                }));

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-响应-标志",
                    RawAddress = "DB200.400.0",
                    TagKind = BuiltinTagKinds.BIT,
                    TagSize = 1,
                }));

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-响应-OK",
                    RawAddress = "DB200.400.1",
                    TagKind = BuiltinTagKinds.BIT,
                    TagSize = 1,
                }));

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-响应-NG",
                    RawAddress = "DB200.400.2",
                    TagKind = BuiltinTagKinds.BIT,
                    TagSize = 1,
                }));
            })
            .Build(channel)
            ;

        var noChannelTag = new NoChannelTag(new TagDescriptor() { 
            TagName = "no-channel-tag",
            TagSize = 1,
            RawAddress = "some-address",
            TagKind = BuiltinTagKinds.BYTE,
        });


        var root = new TagGrp("root", true, channel);
        var grp1 = new TagGrp("sub1", false, null);
        var grp2 = new TagGrp("sub2", false, null);
        root.AddTag(grp1);
        grp2.AddTag(cbnt);
        grp2.AddTag(noChannelTag);
        grp1.AddTag(grp2);

        // 测试层级式访问节点
        var tag1 = root.SelectTag("sub1/sub2/cbnt1/拍照-请求-标志");
        Assert.Equal("拍照-请求-标志", tag1.TagName());
        var tag2 = root.SelectTag("sub1/sub2/cbnt1/拍照-响应-OK");
        Assert.Equal("拍照-响应-OK", tag2.TagName());

        // 测试冒泡式访问通道
        Assert.Equal(cbnt.GetRequiredChannel(), root.Channel);
        Assert.Equal(root.Channel, noChannelTag.GetRequiredChannel());
    }
}
