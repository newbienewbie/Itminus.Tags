using System;
using System.Threading;
using System.Threading.Tasks;
using Itminus.Tags.ModbusTcp;
using Xunit;

namespace Itminus.Tags.Tests.ModbusTags;

public class DITagCbntorTests
{
    private static (TagCbnt cbnt, DITagCbntor tag) CreateContext()
    {
        var cbnt = new TagCbnt(new TagCbntDescriptor { Name = "g", StartAddress = "0" });
        cbnt.ResizeCache(4);
        var descriptor = new TagDescriptor
        {
            TagName = "di1",
            RawAddress = "1~10001",
            TagKind = BuiltinTagKinds.DI,
            TagSize = 1,
        };
        var tag = new DITagCbntor(descriptor, cbnt, 0);
        return (cbnt, tag);
    }

    [Fact]
    public void Value_WhenCacheByteZero_ReturnsFalse()
    {
        var (cbnt, tag) = CreateContext();
        cbnt.Cache.Span[0] = 0x00;

        var result = tag.Value;

        Assert.Equal(false, result);
    }

    [Fact]
    public void Value_WhenCacheByteNonZero_ReturnsTrue()
    {
        var (cbnt, tag) = CreateContext();
        cbnt.Cache.Span[0] = 0x01;

        var result = tag.Value;

        Assert.Equal(true, result);
    }



    /// <summary>
    /// DI是只读的，不可写入
    /// </summary>
    [Fact]
    public void Value_Setter_ThrowsNotSupported()
    {
        var (_, tag) = CreateContext();

        Assert.Throws<NotSupportedException>(() => tag.Value = true);
    }
}
