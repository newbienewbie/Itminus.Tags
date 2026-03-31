using System;
using System.Buffers.Binary;
using System.Linq;
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
        var actual = cbnt.Cache.Span.Slice(0, 4).ToArray();
        Assert.Equal(expected, actual);
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
        var actual = cbnt.Cache.Span.Slice(0, 4).ToArray();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Float_BigEndian_CacheBytesReverseOfLittleEndian()
    {
        var cbnt = CreateCbnt(8);
        var dBig = new TagDescriptor { TagName = "f32", Address = "0", TagKind = BuiltinTagKinds.FLOAT, TagSize = 4, EndianKind = EndianKinds.BigEndian };
        new FloatTagCbntor(dBig, cbnt, 0).Value = 1.0f;

        var dLittle = new TagDescriptor { TagName = "f32", Address = "0", TagKind = BuiltinTagKinds.FLOAT, TagSize = 4, EndianKind = EndianKinds.LittleEndian };
        new FloatTagCbntor(dLittle, cbnt, 4).Value = 2.0f;

        var actual1 = cbnt.Cache.Span.Slice(0, 4).ToArray();
        var actual2 = cbnt.Cache.Span.Slice(4, 4).ToArray();

        Span<byte> expected1 = stackalloc byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(expected1, 1.0f);

        Span<byte> expected2 = stackalloc byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(expected2, 2.0f);

        Assert.Equal(expected1.ToArray().Reverse(), actual1);
        Assert.Equal(expected2.ToArray(), actual2);
    }
}
