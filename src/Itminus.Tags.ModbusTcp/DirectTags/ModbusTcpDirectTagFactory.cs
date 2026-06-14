namespace Itminus.Tags.ModbusTcp;

internal class ModbusTcpDirectTagFactory
{
    public ModbusTcpDirectTagFactory(TagContainer container)
    {
        this._container = container;
    }

    private readonly TagContainer _container;

    public ITag Create(TagDescriptor descriptor, ITagChannel? channel)
    {
        var tag = descriptor.TagKind switch
        {
            // 1000x
            BuiltinTagKinds.DI => CreateDITag(descriptor),
            // 0000x
            BuiltinTagKinds.DO => CreateDOTag(descriptor),

            // each part has 2-words
            BuiltinTagKinds.BIT => CreateBitTag(descriptor),
            //BuiltinTagKinds.BYTE => CreateByteTag(descriptor),
            //BuiltinTagKinds.INT16 => CreateInt16Tag(descriptor),
            //BuiltinTagKinds.UINT16 => CreateUInt16Tag(descriptor),

            //BuiltinTagKinds.INT32 => CreateInt32Tag(descriptor),
            //BuiltinTagKinds.UINT32 => CreateUInt32Tag(descriptor),

            //BuiltinTagKinds.INT64 => CreateInt64Tag(descriptor),
            //BuiltinTagKinds.UINT64 => CreateUInt64Tag(descriptor),

            //BuiltinTagKinds.FLOAT => CreateFloatTag(descriptor)


            _ => throw new Exception($"未预料到的测点种类={descriptor.TagKind}")
        };
        return tag;
    }

    private ITag CreateDITag(TagDescriptor descriptor)
    {
        return new InputContactDirectTag(descriptor, this._container);
    }

    private ITag CreateDOTag(TagDescriptor descriptor)
    {
        return new OutputCoilDirectTag(descriptor, this._container);
    }

    public virtual ITag CreateBitTag(TagDescriptor descriptor)
    {
        // normalize the tagsize
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = 2;
        }
        var tagAddr = ModBusTcpAddressParser.Parse(descriptor.NormalizedAddress);

        if (tagAddr.Area == RegisterKinds.InputRegisters)
        {
            return new InputRegisterBitDirectTag(descriptor, this._container);
        }
        else if (tagAddr.Area == RegisterKinds.HoldingRegisters)
        {
            return new HoldingRegisterBitDirectTag(descriptor, this._container);
        }
        else if(tagAddr.Area == RegisterKinds.InputContacts)
        {
            return new InputContactDirectTag(descriptor, this._container);
        }
        else if (tagAddr.Area == RegisterKinds.OutputCoils)
        {
            return new OutputCoilDirectTag(descriptor, this._container);
        }

        throw new NotImplementedException();
    }
}