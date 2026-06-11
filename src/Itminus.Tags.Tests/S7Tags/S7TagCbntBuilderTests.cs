using Itminus.Tags.S7;
using Xunit;

namespace Itminus.Tags.Tests.S7Tags;

public class S7TagCbntBuilderTests
{
    [Fact]
    public void Build_ShouldNormalizeRelativeAddressesToCbntAreaAndBlock()
    {
        var cbnt = new S7TagCbntBuilder("cbnt1", "DB200.100")
            .Configure(builder =>
            {
                var tagFactory = builder.MakeS7TagFactory();

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "byte-tag",
                    Address = "$$104",
                    TagKind = BuiltinTagKinds.BYTE,
                    TagSize = 1,
                }));

                builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                {
                    TagName = "bit-tag",
                    Address = "$$106.1",
                    TagKind = BuiltinTagKinds.BIT,
                    TagSize = 1,
                }));
            })
            .Build();

        var byteTag = cbnt.SelectTag("byte-tag");
        var bitTag = cbnt.SelectTag("bit-tag");

        Assert.Equal("DB200.104", byteTag.TagAddress());
        Assert.Equal("DB200.106.1", bitTag.TagAddress());
        Assert.Equal(4, byteTag.TagOffset);
        Assert.Equal(6, bitTag.TagOffset);
        Assert.Equal(7, cbnt.CacheSize);
    }
}
