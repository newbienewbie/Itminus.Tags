using System.Buffers.Binary;
using Itminus.Tags;
using Xunit;

namespace Itminus.Tags.Tests.TagCbntors;

public class FloatTagCbntorEndianTests
{
    private static TagCbnt CreateCbnt(int cacheSize)
    {
        var cbnt = new TagCbnt("g", "0");
        cbnt.ResizeCache(cacheSize);
        return cbnt;
    }

    [Theory]
    [InlineData(EndianKinds.LittleEndian)]
    [InlineData(EndianKinds.BigEndian)]
    public void Float_Roundtrip(EndianKinds endian)
    {
        var cbnt = CreateCbnt(8);
        var d = new TagDescriptor { TagName = "f32", Address = "0", TagKind = BuiltinTagKinds.FLOAT, TagSize = 4, EndianKind = endian };
        var tag = new FloatTagCbntor(d, cbnt, 0);

        tag.Value = 1.23456789f;
        Assert.Equal(1.23456789f, (float)tag.Value!);
    }

    [Fact]
    public void Float_BigEndian_WritesFourBytesInBigEndianOrder()
    {
        var cbnt = CreateCbnt(8);
        var d = new TagDescriptor { TagName = "f32", Address = "0", TagKind = BuiltinTagKinds.FLOAT, TagSize = 4, EndianKind = EndianKinds.BigEndian };
        var tag = new FloatTagCbntor(d, cbnt, 0);

        const float value = 1.0f; // 0x3F800000 => big-endian bytes: [0x3F, 0x80, 0x00, 0x00]
        tag.Value = value;

        var expected = new byte[4];
        BinaryPrimitives.WriteSingleBigEndian(expected, value);
        Assert.Equal(expected, cbnt.Cache.Span.Slice(0, 4).ToArray());
    }

    [Fact]
    public void Float_LittleEndian_WritesFourBytesInLittleEndianOrder()
    {
        var cbnt = CreateCbnt(8);
        var d = new TagDescriptor { TagName = "f32", Address = "0", TagKind = BuiltinTagKinds.FLOAT, TagSize = 4, EndianKind = EndianKinds.LittleEndian };
        var tag = new FloatTagCbntor(d, cbnt, 0);

        const float value = 1.0f; // 0x3F800000 => little-endian bytes: [0x00, 0x00, 0x80, 0x3F]
        tag.Value = value;

        var expected = new byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(expected, value);
        Assert.Equal(expected, cbnt.Cache.Span.Slice(0, 4).ToArray());
    }

    [Fact]
    public void Float_BigEndian_CacheBytesReverseOfLittleEndian()
    {
        const float value = 1.0f; // asymmetric IEEE 754 layout ensures bytes differ

        var cbntBig = CreateCbnt(8);
        var dBig = new TagDescriptor { TagName = "f32", Address = "0", TagKind = BuiltinTagKinds.FLOAT, TagSize = 4, EndianKind = EndianKinds.BigEndian };
        new FloatTagCbntor(dBig, cbntBig, 0).Value = value;

        var cbntLittle = CreateCbnt(8);
        var dLittle = new TagDescriptor { TagName = "f32", Address = "0", TagKind = BuiltinTagKinds.FLOAT, TagSize = 4, EndianKind = EndianKinds.LittleEndian };
        new FloatTagCbntor(dLittle, cbntLittle, 0).Value = value;

        var bigBytes = cbntBig.Cache.Span.Slice(0, 4).ToArray();
        var littleBytes = cbntLittle.Cache.Span.Slice(0, 4).ToArray();

        Assert.NotEqual(bigBytes, littleBytes);
        Assert.Equal(bigBytes, littleBytes.Reverse().ToArray());
    }
}
