using Itminus.Tags.DirectTags;

namespace Itminus.Tags.S7;

internal class S7DirectTagFactory
{
    private BitDirectTag CreateBitTag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
    {
        var tagAddr = S7AddressParser.Parse(descriptor.RawAddress);
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = tagAddr.NthBit / 8 + 1;
        }
        return new BitDirectTag(descriptor, thisChannel, channel, nthBit: tagAddr.NthBit);
    }

    private ByteDirectTag CreateByteTag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
    {
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = 1;
        }
        return new ByteDirectTag(descriptor, thisChannel, channel);
    }

    private Int16DirectTag CreateInt16Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
    {
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = 2;
        }
        return new Int16DirectTag(descriptor, thisChannel, channel);
    }

    private UInt16DirectTag CreateUInt16Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
    {
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = 2;
        }
        return new UInt16DirectTag(descriptor, thisChannel, channel);
    }


    private Int32DirectTag CreateInt32Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
    {
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = 4;
        }
        return new Int32DirectTag(descriptor, thisChannel, channel);
    }

    private UInt32DirectTag CreateUInt32Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
    {
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = 4;
        }
        return new UInt32DirectTag(descriptor, thisChannel, channel);
    }


    private Int64DirectTag CreateInt64Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
    {
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = 8;
        }
        return new Int64DirectTag(descriptor, thisChannel, channel);
    }


    private UInt64DirectTag CreateUInt64Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
    {
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = 8;
        }
        return new UInt64DirectTag(descriptor, thisChannel, channel);
    }


    private FloatDirectTag CreateFloatTag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
    {
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = 4;
        }
        return new FloatDirectTag(descriptor, thisChannel, channel);
    }


    private StrDirectTag CreateStrTag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
    {
        S7Utils.NormalizeS7StrTagSize(descriptor, out byte maxlen);
        return new StrDirectTag(descriptor, thisChannel, channel, maxLen: maxlen);
    }

    public ITag Create(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
    {
        ITag tag = descriptor.TagKind switch
        {
            BuiltinTagKinds.BIT => CreateBitTag(descriptor, thisChannel, channel),
            BuiltinTagKinds.BYTE => CreateByteTag(descriptor, thisChannel, channel),
            BuiltinTagKinds.INT16 => CreateInt16Tag(descriptor, thisChannel, channel),
            BuiltinTagKinds.UINT16 => CreateUInt16Tag(descriptor, thisChannel, channel),
            BuiltinTagKinds.INT32 => CreateInt32Tag(descriptor, thisChannel, channel),
            BuiltinTagKinds.UINT32 => CreateUInt32Tag(descriptor, thisChannel, channel),
            BuiltinTagKinds.INT64 => CreateInt64Tag(descriptor, thisChannel, channel),
            BuiltinTagKinds.UINT64 => CreateUInt64Tag(descriptor, thisChannel, channel),
            BuiltinTagKinds.FLOAT => CreateFloatTag(descriptor, thisChannel, channel),
            BuiltinTagKinds.STR => CreateStrTag(descriptor, thisChannel, channel),
            _ => throw new Exception($"未预料到的测点种类={descriptor.TagKind}")
        };
        return tag;

    }

}
