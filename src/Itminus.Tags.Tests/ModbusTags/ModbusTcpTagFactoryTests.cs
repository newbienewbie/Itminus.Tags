using Itminus.Tags.ModbusTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.ModbusTags;

public class ModbusTcpTagFactoryTests
{
    [Theory]

    [InlineData("40001", "1~40007.0", 12, 12)]  // 0~7，处于第一个字节
    [InlineData("40001", "1~40007.1", 12, 12)]
    [InlineData("40001", "1~40007.2", 12, 12)]
    [InlineData("40001", "1~40007.3", 12, 12)]
    [InlineData("40001", "1~40007.4", 12, 12)]
    [InlineData("40001", "1~40007.5", 12, 12)]
    [InlineData("40001", "1~40007.6", 12, 12)]
    [InlineData("40001", "1~40007.7", 12, 12)]
    [InlineData("40001", "1~40007.8", 12, 13)]  // 8~15 位于第二个字节

    [InlineData("40001", "1~40008.1", 14, 14)]
    [InlineData("40001", "1~40009.1", 16, 16)]
    [InlineData("40001", "1~40009.15",16, 17)]
    [InlineData("40011", "40021.7", 20, 20)]
    public void Test_HoldingRegisters_BitTagOffset(string baseAddr, string bitAddr,int tagOffset, int cacheOffset)
    {
        var cbntDesc = new TagCbntDescriptor { Name = "g1", StartAddress = baseAddr };
        var builder = new ModbusTcpTagCbntBuilder()
            .WithCbntDescriptor(cbntDesc)
            .WithChannel(null!);
        var factory = new ModbusTcpTagFactory(builder);
        var bittag = factory.CreateBitTag(new TagDescriptor() { TagName = bitAddr, RawAddress = bitAddr, TagKind = BuiltinTagKinds.BIT, TagSize = 2 });
        Assert.Equal(cacheOffset, bittag.CacheOffset);
        Assert.Equal(tagOffset, bittag.TagOffset);
        Assert.Equal(2, bittag.TagSize());
    }

    [Theory]
    [InlineData("10011", "10021", 10, 10)]
    [InlineData("1~00011", "00021", 10, 10)]
    public void Test_Input_BitTagOffset(string baseAddr, string bitAddr, int tagOffset, int cacheOffset)
    {
        var cbntDesc = new TagCbntDescriptor { Name = "g1", StartAddress = baseAddr };
        var builder = new ModbusTcpTagCbntBuilder()
            .WithCbntDescriptor(cbntDesc)
            .WithChannel(null!);
        var factory = new ModbusTcpTagFactory(builder);
        var bittag = factory.CreateBitTag(new TagDescriptor() { TagName = bitAddr, RawAddress = bitAddr, TagKind = BuiltinTagKinds.BIT, TagSize = 1 });
        Assert.Equal(cacheOffset, bittag.CacheOffset);
        Assert.Equal(tagOffset, bittag.TagOffset);
        Assert.Equal(1, bittag.TagSize());
    }



    [Theory]
    [InlineData("40001", "1~40007", 12)]
    [InlineData("40001", "40030", 58)]
    public void Test_Int16TagOffset(string baseAddr, string tagAddr, int offset)
    {
        var cbntDesc = new TagCbntDescriptor { Name = "g1", StartAddress = baseAddr };
        var builder = new ModbusTcpTagCbntBuilder()
            .WithCbntDescriptor(cbntDesc)
            .WithChannel(null!);
        var factory = new ModbusTcpTagFactory(builder);
        var bittag = factory.CreateBitTag(new TagDescriptor() { TagName = tagAddr, RawAddress = tagAddr, TagKind = BuiltinTagKinds.UINT16, TagSize = 2 });
        Assert.Equal(offset, bittag.CacheOffset);
        Assert.Equal(offset, bittag.TagOffset);
    }



    [Theory]
    [InlineData("40001", "1~40007", 12)]
    [InlineData("40001", "40030", 58)]
    public void Test_FloatTagOffset(string baseAddr, string tagAddr, int offset)
    {
        var cbntDesc = new TagCbntDescriptor { Name = "g1", StartAddress = baseAddr };
        var builder = new ModbusTcpTagCbntBuilder()
            .WithCbntDescriptor(cbntDesc)
            .WithChannel(null!); 
        var factory = new ModbusTcpTagFactory(builder);
        var bittag = factory.CreateBitTag(new TagDescriptor() { TagName = tagAddr, RawAddress = tagAddr, TagKind = BuiltinTagKinds.FLOAT, TagSize = 4 });
        Assert.Equal(offset, bittag.CacheOffset);
        Assert.Equal(offset, bittag.TagOffset);
    }
}
