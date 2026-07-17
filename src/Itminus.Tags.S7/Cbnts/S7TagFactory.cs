using Itminus.Tags.TagCbntors;

namespace Itminus.Tags.S7;


/// <summary>
/// S7 测点工厂
/// </summary>
public class S7TagFactory : TagCbntorFactoryBase
{
    /// <summary>
    /// c'tor
    /// </summary>
    public S7TagFactory(TagCbntBuilderBase builder) : base(builder)
    { 
    }

    /// <summary>
    /// 获取测点偏移
    /// </summary>
    protected int GetTagOffset(TagDescriptor tagDescriptor, out S7Address tagAddr)
    {
        tagAddr = S7AddressParser.Parse(tagDescriptor.RawAddress);
        var groupAddr = S7AddressParser.Parse(this.TagCbnt.StartAddress);

        var offset = tagAddr.StartAddress - groupAddr.StartAddress;
        return offset;
    }


    #region
    /// <summary>
    /// 创建 Bit型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    protected virtual BitTagCbntor CreateBitTag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 1;
        }

        var tagAddr = S7AddressParser.Parse(tagDescriptor.RawAddress);
        var groupAddr = S7AddressParser.Parse(this.TagCbnt.StartAddress);

        var offset = tagAddr.StartAddress -  groupAddr.StartAddress;
        if (tagAddr.NthBit < 8)
        {
            return new BitTagCbntor(tagDescriptor, this.TagCbnt, offset, offset, tagAddr.NthBit);
        }
        else
        {
            var nth = tagAddr.NthBit % 8;
            return new BitTagCbntor(tagDescriptor, this.TagCbnt, offset, offset + 1, (byte)nth);
        }
    }

    /// <summary>
    /// 创建 Byte型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    protected virtual ByteTagCbntor CreateByteTag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 1;
        }

        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new ByteTagCbntor(tagDescriptor, this.TagCbnt, offset);
    }

    /// <summary>
    /// 创建 Int16型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    protected virtual Int16TagCbntor CreateInt16Tag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 2;
        }

        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new Int16TagCbntor(tagDescriptor, this.TagCbnt, offset);
    }

    /// <summary>
    ///  创建 UInt16型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    protected virtual UInt16TagCbntor CreateUInt16Tag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 2;
        }

        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new UInt16TagCbntor(tagDescriptor, this.TagCbnt, offset);
    }

    /// <summary>
    /// 创建 Int32 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    protected virtual Int32TagCbntor CreateInt32Tag(TagDescriptor tagDescriptor)
    {   
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 4;
        }
        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new Int32TagCbntor(tagDescriptor, this.TagCbnt, offset);
    }

    /// <summary>
    /// 创建 UInt32 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    protected virtual UInt32TagCbntor CreateUInt32Tag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 4;
        }

        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new UInt32TagCbntor(tagDescriptor, this.TagCbnt, offset);
    }

    /// <summary>
    /// 创建 Int64 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    protected virtual Int64TagCbntor CreateInt64Tag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 8;
        }

        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new Int64TagCbntor(tagDescriptor, this.TagCbnt, offset);
    }

    /// <summary>
    /// 创建 UInt64 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    protected virtual UInt64TagCbntor CreateUInt64Tag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 8;
        }

        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new UInt64TagCbntor(tagDescriptor, this.TagCbnt, offset);
    }

    /// <summary>
    /// 创建 float 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    protected virtual FloatTagCbntor CreateFloatTag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 4;
        }

        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new FloatTagCbntor(tagDescriptor, this.TagCbnt, offset);
    }


    /// <summary>
    /// 创建 STR 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    protected virtual S7StrTagCbntor CreateStrTag(TagDescriptor tagDescriptor)
    {
        S7Utils.NormalizeS7StrTagSize(tagDescriptor, out var maxlen);
        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new S7StrTagCbntor(tagDescriptor, this.TagCbnt, offset, maxlen);
    }
    #endregion

    /// <inheritdoc/>
    public override ITagCbntor CreateTag(TagDescriptor descriptor)
    {
        ITagCbntor tag = descriptor.TagKind switch
        {
            BuiltinTagKinds.BIT => CreateBitTag(descriptor),
            BuiltinTagKinds.BYTE => CreateByteTag(descriptor),
            BuiltinTagKinds.INT16 => CreateInt16Tag(descriptor),
            BuiltinTagKinds.UINT16 => CreateUInt16Tag(descriptor),
            BuiltinTagKinds.INT32 => CreateInt32Tag(descriptor),
            BuiltinTagKinds.UINT32 => CreateUInt32Tag(descriptor),
            BuiltinTagKinds.INT64 => CreateInt64Tag(descriptor),
            BuiltinTagKinds.UINT64 => CreateUInt64Tag(descriptor),
            BuiltinTagKinds.FLOAT => CreateFloatTag(descriptor),
            BuiltinTagKinds.STR => CreateStrTag(descriptor),
            _ => throw new Exception($"未预料到的测点种类={descriptor.TagKind}")
        };
        return tag;
    }

}
