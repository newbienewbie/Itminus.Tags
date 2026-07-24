using System;
using System.Threading;
using System.Threading.Tasks;
using Itminus.Tags.ModbusTcp;
using Xunit;

namespace Itminus.Tags.Tests.ModbusTags;

public class DOTagCbntorTests
{
    private static (TagCbnt cbnt, DOTagCbntor tag) CreateContext()
    {
        var cbnt = new TagCbnt(new TagCbntDescriptor { Name = "g", StartAddress = "0" });
        cbnt.ResizeCache(4);
        var descriptor = new TagDescriptor
        {
            TagName = "do1",
            RawAddress = "1~00001",
            TagKind = BuiltinTagKinds.DO,
            TagSize = 1,
        };
        var tag = new DOTagCbntor(descriptor, cbnt, 0);
        return (cbnt, tag);
    }

    [Fact]
    public void Value_WhenCacheByteZero_ReturnsFalse()
    {
        var (cbnt, tag) = CreateContext();
        cbnt.Cache.Span[0] = 0x00;

        Assert.Equal(false, tag.Value);
    }

    [Fact]
    public void Value_WhenCacheByteNonZero_ReturnsTrue()
    {
        var (cbnt, tag) = CreateContext();
        cbnt.Cache.Span[0] = 0x01;

        Assert.Equal(true, tag.Value);
    }

    [Fact]
    public void Value_SetTrue_WritesToCache()
    {
        var (cbnt, tag) = CreateContext();

        tag.Value = true;

        Assert.Equal((byte)1, cbnt.Cache.Span[0]);
    }

    [Fact]
    public void Value_SetFalse_WritesToCache()
    {
        var (cbnt, tag) = CreateContext();
        cbnt.Cache.Span[0] = 0x01;

        tag.Value = false;

        Assert.Equal((byte)0, cbnt.Cache.Span[0]);
    }

    [Fact]
    public void Value_SetNonBool_Throws()
    {
        var (_, tag) = CreateContext();

        var ex = Assert.Throws<Exception>(() => tag.Value = 42);
        Assert.Contains("Bit类型", ex.Message);
    }

    [Fact]
    public void Value_Set_MarksDirty()
    {
        var (cbnt, tag) = CreateContext();
        // 初始时已读取过，设置成同样的值
        cbnt.Cache.Span[0] = 0x00;

        tag.Value = true;

        // Value setter 总会调用 MarkDirty
        Assert.True(tag.IsDirty);
        Assert.True(cbnt.IsDirty);
    }

    [Fact]
    public void Value_Set_UpdatesTimestamp()
    {
        var (_, tag) = CreateContext();

        tag.Value = true;

        Assert.NotEqual(default, tag.Timestamp);
    }
}
