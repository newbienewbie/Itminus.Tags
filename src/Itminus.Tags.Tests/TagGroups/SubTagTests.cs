using Itminus.Tags.S7;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.TagGroups;

public class SubTagTests
{
    [Fact]
    public void Test()
    {
        var loggerFactory = new LoggerFactory();

        var channel = new S7TagChannel(
                "S7",
                new StdUnit.Sharp7.Options.S7PlcItem() { IpAddr = "localhost", Rack = 0, Slot = 1 },
                loggerFactory.CreateLogger<S7TagChannel>()
            );
        

        var cbnt = new S7TagCbntBuilder("cbnt1", "DB200.100.1")
            .Configure(builder =>
            {
                var tagFactory = builder.MakeS7TagFactory();

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-请求-标志",
                    Address = "DB200.100.1",
                    TagKind = TagKinds.BIT,
                    TagSize = 1,
                }));

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-请求-料号",
                    Address = "DB200.102",
                    TagKind = TagKinds.BYTE,
                    TagSize = 1,
                }));

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-请求-程序号",
                    Address = "DB200.104",
                    TagKind = TagKinds.INT16,
                    TagSize = 2,
                }));

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-响应-标志",
                    Address = "DB200.400.0",
                    TagKind = TagKinds.BIT,
                    TagSize = 1,
                }));

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-响应-OK",
                    Address = "DB200.400.1",
                    TagKind = TagKinds.BIT,
                    TagSize = 1,
                }));

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "拍照-响应-NG",
                    Address = "DB200.400.2",
                    TagKind = TagKinds.BIT,
                    TagSize = 1,
                }));
            })
            .Build()
            ;


        var root = new TagGrp("root", true, channel);
        var grp1 = new TagGrp("sub1", false, null);
        var grp2 = new TagGrp("sub2", false, null);
        root.AddTag(grp1);
        grp2.AddTag(cbnt);
        grp1.AddTag(grp2);

        // 测试层级式访问节点
        var tag1 = root.SelectTag("sub1/sub2/cbnt1/拍照-请求-标志");
        Assert.Equal("拍照-请求-标志", tag1.TagName());
        var tag2 = root.SelectTag("sub1/sub2/cbnt1/拍照-响应-OK");
        Assert.Equal("拍照-响应-OK", tag2.TagName());

        // 测试冒泡式访问通道
        Assert.Equal(cbnt.GetRequiredChannel(), root.Channel);
    }
}
