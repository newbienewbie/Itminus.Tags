namespace Itminus.Tags.ModbusTcp;

internal class ModbusTcpDirectTagFactory
{
    public ModbusTcpDirectTagFactory(TagContainer container)
    {
        this._container = container;
    }

    private readonly TagContainer _container;

    public ITag Create(TagDescriptor descriptor, ModbusTcpChannel? channel)
    {
        var addr = ModBusTcpAddressParser.Parse(descriptor.NormalizedAddress);
        var tag = descriptor.TagKind switch
        {
            // 1000x
            BuiltinTagKinds.DI => CreateDITag(descriptor, addr, channel),
            // 0000x
            BuiltinTagKinds.DO => CreateDOTag(descriptor, addr, channel),
            // each part has 2-words
            BuiltinTagKinds.BIT => CreateBitTag(descriptor, addr, channel),

            BuiltinTagKinds.BYTE => CreateByteTag(descriptor, addr, channel),

            //BuiltinTagKinds.INT16 => CreateInt16Tag(descriptor),
            BuiltinTagKinds.UINT16 => CreateUShortTag(descriptor, addr, channel),

            //BuiltinTagKinds.INT32 => CreateInt32Tag(descriptor),
            //BuiltinTagKinds.UINT32 => CreateUInt32Tag(descriptor),

            //BuiltinTagKinds.INT64 => CreateInt64Tag(descriptor),
            //BuiltinTagKinds.UINT64 => CreateUInt64Tag(descriptor),

            //BuiltinTagKinds.FLOAT => CreateFloatTag(descriptor)


            _ => throw new Exception($"未预料到的测点种类={descriptor.TagKind}")
        };
        return tag;
    }

    #region BitLikes
    private ITag CreateDITag(TagDescriptor descriptor, ModbusTcpAddress addr, ModbusTcpChannel? thisChannel)
    {
        return new InputContactDirectTag(descriptor, thisChannel, this._container);
    }

    private ITag CreateDOTag(TagDescriptor descriptor, ModbusTcpAddress addr, ModbusTcpChannel? thisChannel)
    {
        return new OutputCoilDirectTag(descriptor, thisChannel, this._container);
    }

    public virtual ITag CreateBitTag(TagDescriptor descriptor, ModbusTcpAddress addr, ModbusTcpChannel? thisChannel)
    {
        // normalize the tagsize
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = 2;
        }

        if (addr.Area == RegisterKinds.InputRegisters)
        {
            return new InputRegisterBitDirectTag(descriptor, thisChannel, this._container);
        }
        else if (addr.Area == RegisterKinds.HoldingRegisters)
        {
            return new HoldingRegisterBitDirectTag(descriptor, thisChannel, this._container);
        }
        else if(addr.Area == RegisterKinds.InputContacts)
        {
            return new InputContactDirectTag(descriptor, thisChannel, this._container);
        }
        else if (addr.Area == RegisterKinds.OutputCoils)
        {
            return new OutputCoilDirectTag(descriptor, thisChannel, this._container);
        }

        throw new NotImplementedException();
    }
    #endregion


    private ITag CreateByteTag(TagDescriptor descriptor, ModbusTcpAddress addr, ModbusTcpChannel? thisChannel)
    {
        return new ByteDirectTag(descriptor, thisChannel, this._container );
    }

    private ITag CreateUShortTag(TagDescriptor descriptor, ModbusTcpAddress addr, ModbusTcpChannel? thisChannel)
    {
        // 目前仅支持保持寄存器，输入寄存器的直接测点留待以后实现
        if (addr.Area != RegisterKinds.HoldingRegisters)
        {
            throw new NotImplementedException($"测点配置的寄存器类型暂不支持，请考虑使用连续测点。(Tag={descriptor.TagName})");
        }
        return new UInt16DirectTag(descriptor, thisChannel, this._container);
    }
}