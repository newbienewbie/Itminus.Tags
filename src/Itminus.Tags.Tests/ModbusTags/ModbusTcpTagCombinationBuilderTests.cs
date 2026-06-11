using Itminus.Tags.ModbusTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.ModbusTags;

public class ModbusTcpTagCombinationBuilderTests
{
    [Fact]
    public void Test_TagCombinationCacheSize()
    {
        var builder = new ModbusTcpTagCbntBuilder("g1", "40001")
            .WithChannel(null!)
            .WithInterval(100)
            .WithIsEnabled(true);

        builder.AddTags(new List<TagDescriptor> {
            new TagDescriptor{
                TagName ="Float1",
                RawAddress = "40001",
                TagKind = BuiltinTagKinds.FLOAT,
                TagSize = 4,
            },
            new TagDescriptor{
                TagName ="Float2",
                RawAddress = "40003",
                TagKind = BuiltinTagKinds.FLOAT,
                TagSize = 4,
            },
            // leave 40005 empty
            new TagDescriptor{
                TagName ="Bit1",
                RawAddress = "40006.1",
                TagKind = BuiltinTagKinds.BIT,
                TagSize = 2,
            },
            new TagDescriptor{
                TagName ="Bit2",
                RawAddress = "40006.2",
                TagKind = BuiltinTagKinds.BIT,
                TagSize = 2,
            },
            new TagDescriptor{
                TagName ="Bit3",
                RawAddress = "40006.15",
                TagKind = BuiltinTagKinds.BIT,
                TagSize = 2,
            },
            new TagDescriptor{
                TagName ="UInt1",
                RawAddress = "40007",
                TagKind = BuiltinTagKinds.UINT16,
                TagSize = 2,
            },
            // leave 40008 empty
            new TagDescriptor{
                TagName ="UInt2",
                RawAddress = "40009",
                TagKind = BuiltinTagKinds.UINT16,
                TagSize = 2,
            },
            new TagDescriptor{
                TagName ="Bit4",
                RawAddress = "40010.15",
                TagKind = BuiltinTagKinds.BIT,
                TagSize = 2,
            },
        });

        var cbnt = builder.Build();
        Assert.Equal( 20 , cbnt.CacheSize);
    }



}
