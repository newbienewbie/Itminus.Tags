using Itminus.Tags.ModbusTcp.Tags;

namespace Itminus.Tags.ModbusTcp;



public class ModbusTcpTagFactory : TagCbntorFactoryBase
{
    public ModbusTcpTagFactory(TagCbntBuilderBase builder) : base(builder)
    {
    }


    protected override int GetTagOffset(TagDescriptor tagDescriptor)
    {
        var tagAddr = ModBusTcpAddressParser.Parse(tagDescriptor.Address);
        var groupAddr = ModBusTcpAddressParser.Parse(TagCbnt.StartAddress);

        var offset = tagAddr.StartPoint - groupAddr.StartPoint;
        return offset * 2;
    }

    #region
    public virtual DITag CreateDITag(TagDescriptor tagDescriptor)
    {
        var tagAddr = ModBusTcpAddressParser.Parse(tagDescriptor.Address);
        var groupAddr = ModBusTcpAddressParser.Parse(TagCbnt.StartAddress);

        if (tagAddr.Area != RegisterKinds.InputContacts)
        {
            throw new Exception($"地址区域{tagAddr.Area}不可作为DI测点");
        }
        var offset = tagAddr.StartPoint - groupAddr.StartPoint;
        return new DITag(tagDescriptor, TagCbnt, offset);
    }


    public virtual DOTag CreateDOTag(TagDescriptor tagDescriptor)
    {
        var tagAddr = ModBusTcpAddressParser.Parse(tagDescriptor.Address);
        var groupAddr = ModBusTcpAddressParser.Parse(TagCbnt.StartAddress);

        if (tagAddr.Area != RegisterKinds.OutputCoils)
        {
            throw new Exception($"地址区域{tagAddr.Area}不可作为DO测点");
        }
        var offset = tagAddr.StartPoint - groupAddr.StartPoint;
        return new DOTag(tagDescriptor, TagCbnt, offset);
    }
    #endregion

    public virtual BitTagCbntor CreateBitTag(TagDescriptor tagDescriptor)
    {
        var tagAddr = ModBusTcpAddressParser.Parse(tagDescriptor.Address);
        var groupAddr = ModBusTcpAddressParser.Parse(TagCbnt.StartAddress);

        if (tagAddr.Area == RegisterKinds.InputRegisters || tagAddr.Area == RegisterKinds.HoldingRegisters)
        {
            var offset = tagAddr.StartPoint - groupAddr.StartPoint;
            offset *= 2;
            if (tagAddr.NthBit < 8)
            {
                return new BitTagCbntor(tagDescriptor, TagCbnt, offset, offset, tagAddr.NthBit);
            }
            else
            {
                var nth = tagAddr.NthBit % 8;
                return new BitTagCbntor(tagDescriptor, TagCbnt, offset, offset + 1, (byte)nth);
            }
        }
        else if (tagAddr.Area == RegisterKinds.InputContacts || tagAddr.Area == RegisterKinds.OutputCoils)
        {
            var offset = tagAddr.StartPoint - groupAddr.StartPoint;
            return new BitTagCbntor(tagDescriptor, TagCbnt, offset, offset, 0);
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// 创建 Byte型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual ByteTagCbntor CreateByteTag(TagDescriptor tagDescriptor)
    {
        int offset = GetTagOffset(tagDescriptor);
        return new ByteTagCbntor(tagDescriptor, TagCbnt, offset);
    }

    /// <summary>
    /// 创建 Int16型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual Int16TagCbntor CreateInt16Tag(TagDescriptor tagDescriptor)
    {
        var offset = GetTagOffset(tagDescriptor);
        return new Int16TagCbntor(tagDescriptor, TagCbnt, offset);
    }


    /// <summary>
    /// 创建 UInt16型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual UInt16TagCbntor CreateUInt16Tag(TagDescriptor tagDescriptor)
    {
        var offset = GetTagOffset(tagDescriptor);
        return new UInt16TagCbntor(tagDescriptor, TagCbnt, offset);
    }

    /// <summary>
    /// 创建 Int32 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual Int32TagCbntor CreateInt32Tag(TagDescriptor tagDescriptor)
    {
        var offset = GetTagOffset(tagDescriptor);
        return new Int32TagCbntor(tagDescriptor, TagCbnt, offset);
    }

    /// <summary>
    /// 创建 UInt32 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual UInt32TagCbntor CreateUInt32Tag(TagDescriptor tagDescriptor)
    {
        var offset = GetTagOffset(tagDescriptor);
        return new UInt32TagCbntor(tagDescriptor, TagCbnt, offset);
    }

    /// <summary>
    /// 创建 float 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual FloatTagCbntor CreateFloatTag(TagDescriptor tagDescriptor)
    {
        var offset = GetTagOffset(tagDescriptor);
        return new FloatTagCbntor(tagDescriptor, TagCbnt, offset);
    }

    public override ITagCbntor CreateTag(TagDescriptor descriptor)
    {
        var tag = descriptor.TagKind switch
        {
            TagKinds.BIT => CreateBitTag(descriptor),
            TagKinds.BYTE => CreateByteTag(descriptor) as ITagCbntor,
            TagKinds.INT16 => CreateInt16Tag(descriptor) as ITagCbntor,
            TagKinds.UINT16 => CreateUInt16Tag(descriptor) as ITagCbntor,
            TagKinds.INT32 => CreateInt32Tag(descriptor) as ITagCbntor,
            TagKinds.UINT32 => CreateUInt32Tag(descriptor) as ITagCbntor,
            TagKinds.FLOAT => CreateFloatTag(descriptor) as ITagCbntor,
            _ => throw new Exception($"未预料到的测点种类={descriptor.TagKind}")
        };
        return tag;
    }

}
