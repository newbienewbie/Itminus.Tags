namespace Itminus.Tags.S7;



public class S7TagFactory : TagCbntorFactoryBase
{
    public S7TagFactory(TagCbntBuilderBase builder) : base(builder)
    { 
    }


    protected override int GetTagOffset(TagDescriptor tagDescriptor)
    {
        var tagAddr = S7AddressParser.Parse(tagDescriptor.Address);
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

        var tagAddr = S7AddressParser.Parse(tagDescriptor.Address);
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

        int offset = GetTagOffset(tagDescriptor);
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

        var offset = GetTagOffset(tagDescriptor);
        return new Int16TagCbntor(tagDescriptor, this.TagCbnt, offset);
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

        var offset = GetTagOffset(tagDescriptor);
        return new Int32TagCbntor(tagDescriptor, this.TagCbnt, offset);
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

        var offset = GetTagOffset(tagDescriptor);
        return new FloatTagCbntor(tagDescriptor, this.TagCbnt, offset);
    }
    #endregion

    public override ITagCbntor CreateTag(TagDescriptor descriptor)
    {
        var tag = descriptor.TagKind switch
        {
            TagKinds.BIT => CreateBitTag(descriptor),
            TagKinds.BYTE => CreateByteTag(descriptor),
            TagKinds.INT16 => CreateInt16Tag(descriptor),
            TagKinds.INT32 => CreateInt32Tag(descriptor),
            TagKinds.FLOAT => CreateFloatTag(descriptor) as ITagCbntor,
            _ => throw new Exception($"未预料到的测点种类={descriptor.TagKind}")
        };
        return tag;
    }

}
