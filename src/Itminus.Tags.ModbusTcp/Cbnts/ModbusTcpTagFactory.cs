
using Itminus.Tags.TagCbntors;

namespace Itminus.Tags.ModbusTcp;


/// <summary>
/// ModbusTcp 测点工厂
/// </summary>
public class ModbusTcpTagFactory : TagCbntorFactoryBase
{
    /// <summary>
    /// c'tor
    /// </summary>
    public ModbusTcpTagFactory(TagCbntBuilderBase builder) : base(builder)
    {
    }

    /// <summary>
    /// 获取测点偏移量
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <param name="tagAddr"></param>
    /// <returns></returns>
    protected int GetTagOffset(TagDescriptor tagDescriptor, out ModbusTcpAddress tagAddr)
    {
        tagAddr = ModBusTcpAddressParser.Parse(tagDescriptor.RawAddress);
        var groupAddr = ModBusTcpAddressParser.Parse(TagCbnt.StartAddress);

        var offset = tagAddr.StartPoint - groupAddr.StartPoint;
        return offset * 2;
    }

    #region
    /// <summary>
    /// 创建DI测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public virtual DITagCbntor CreateDITag(TagDescriptor tagDescriptor)
    {           
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 1;
        }

        var tagAddr = ModBusTcpAddressParser.Parse(tagDescriptor.RawAddress);
        var groupAddr = ModBusTcpAddressParser.Parse(TagCbnt.StartAddress);

        if (tagAddr.Area != RegisterKinds.InputContacts)
        {
            throw new Exception($"地址区域{tagAddr.Area}不可作为DI测点");
        }
        var offset = tagAddr.StartPoint - groupAddr.StartPoint;
        return new DITagCbntor(tagDescriptor, TagCbnt, offset);
    }

    /// <summary>
    /// 创建DO测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public virtual DOTagCbntor CreateDOTag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 1;
        }

        var tagAddr = ModBusTcpAddressParser.Parse(tagDescriptor.NormalizedAddress);
        var groupAddr = ModBusTcpAddressParser.Parse(TagCbnt.StartAddress);

        if (tagAddr.Area != RegisterKinds.OutputCoils)
        {
            throw new Exception($"地址区域{tagAddr.Area}不可作为DO测点");
        }
        var offset = tagAddr.StartPoint - groupAddr.StartPoint;
        return new DOTagCbntor(tagDescriptor, TagCbnt, offset);
    }
    #endregion

    /// <summary>
    /// 创建 Bit型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual BitTagCbntor CreateBitTag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 2;
        }
        var tagAddr = ModBusTcpAddressParser.Parse(tagDescriptor.NormalizedAddress);
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
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 2;
        }
        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new ByteTagCbntor(tagDescriptor, TagCbnt, offset);
    }

    /// <summary>
    /// 创建 Int16型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual Int16TagCbntor CreateInt16Tag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 2;
        }
        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new Int16TagCbntor(tagDescriptor, TagCbnt, offset);
    }


    /// <summary>
    /// 创建 UInt16型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual UInt16TagCbntor CreateUInt16Tag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 2;
        }
        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new UInt16TagCbntor(tagDescriptor, TagCbnt, offset);
    }

    /// <summary>
    /// 创建 Int32 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual Int32TagCbntor CreateInt32Tag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 4;
        }
        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new Int32TagCbntor(tagDescriptor, TagCbnt, offset);
    }

    /// <summary>
    /// 创建 UInt32 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual UInt32TagCbntor CreateUInt32Tag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 4;
        }
        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new UInt32TagCbntor(tagDescriptor, TagCbnt, offset);
    }

    /// <summary>
    /// 创建 Int64 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual Int64TagCbntor CreateInt64Tag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 8;
        }
        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new Int64TagCbntor(tagDescriptor, TagCbnt, offset);
    }

    /// <summary>
    /// 创建 UInt64 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual UInt64TagCbntor CreateUInt64Tag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 8;
        }
        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new UInt64TagCbntor(tagDescriptor, TagCbnt, offset);
    }

    /// <summary>
    /// 创建 float 型测点
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public virtual FloatTagCbntor CreateFloatTag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 4;
        }
        int offset = GetTagOffset(tagDescriptor, out var tagAddr);
        return new FloatTagCbntor(tagDescriptor, TagCbnt, offset);
    }


    /// <inheritdoc/>
    public override ITagCbntor CreateTag(TagDescriptor descriptor)
    {
        var tag = descriptor.TagKind switch
        {
            // 1000x
            BuiltinTagKinds.DI => CreateDITag(descriptor) as ITagCbntor,
            // 0000x
            BuiltinTagKinds.DO => CreateDOTag(descriptor) as ITagCbntor,

            // each part has 2-words
            BuiltinTagKinds.BIT => CreateBitTag(descriptor),
            BuiltinTagKinds.BYTE => CreateByteTag(descriptor) as ITagCbntor,
            BuiltinTagKinds.INT16 => CreateInt16Tag(descriptor) as ITagCbntor,
            BuiltinTagKinds.UINT16 => CreateUInt16Tag(descriptor) as ITagCbntor,

            BuiltinTagKinds.INT32 => CreateInt32Tag(descriptor) as ITagCbntor,
            BuiltinTagKinds.UINT32 => CreateUInt32Tag(descriptor) as ITagCbntor,

            BuiltinTagKinds.INT64 => CreateInt64Tag(descriptor) as ITagCbntor,
            BuiltinTagKinds.UINT64 => CreateUInt64Tag(descriptor) as ITagCbntor,


            BuiltinTagKinds.FLOAT => CreateFloatTag(descriptor) as ITagCbntor,


            _ => throw new Exception($"未预料到的测点种类={descriptor.TagKind}")
        };
        return tag;
    }

}
