
namespace Itminus.Tags.S7;

internal class S7DirectTagBuilder : TagBuilderBase
{
    public S7DirectTagBuilder()
    {
    }


    public override ITag Build(ITagChannel channel)
    {

        if (this.Channel is not null && this.Channel is not S7TagChannel)
        {
            throw new Exception($"测点({this.Name})配置了通道，但不是{nameof(S7TagChannel)}");
        }

        var factory = new S7DirectTagFactory();
       
        // 这个时候 tag 尚未构造完成，尚未关联到 Parent，不可以使用 tag.GetRequiredChannel()，必须依赖外部传入的 channel 参数
        var s7Channel = channel as S7TagChannel;
        if(s7Channel is null)
        {
            throw new Exception($"测点({this.Name})配置的通道不是{nameof(S7TagChannel)}");
        }
        var tag = factory.Create(this.TagDescriptor, this.Channel, s7Channel);

        return tag;
    }


    class S7DirectTagFactory
    {
        private BitTag CreateBitTag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
        {
            var tagAddr = S7AddressParser.Parse(descriptor.RawAddress);
            if (descriptor.TagSize == 0)
            {
                descriptor.TagSize = tagAddr.NthBit / 8 + 1;
            }
            return new BitTag(descriptor, thisChannel, channel, nthBit: tagAddr.NthBit);
        }

        private ByteTag CreateByteTag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
        {
            if(descriptor.TagSize == 0)
            {
                descriptor.TagSize = 1;
            }
            return new ByteTag(descriptor, thisChannel, channel);
        }

        private Int16Tag CreateInt16Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
        {
            if (descriptor.TagSize == 0)
            {
                descriptor.TagSize = 2;
            }
            return new Int16Tag(descriptor, thisChannel, channel);
        }

        private UInt16Tag CreateUInt16Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
        {
            if (descriptor.TagSize == 0)
            {
                descriptor.TagSize = 2;
            }
            return new UInt16Tag(descriptor, thisChannel, channel);
        }


        private Int32Tag CreateInt32Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
        {
            if (descriptor.TagSize == 0)
            {
                descriptor.TagSize = 4;
            }
            return new Int32Tag(descriptor, thisChannel, channel);
        }

        private UInt32Tag CreateUInt32Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
        {
            if (descriptor.TagSize == 0)
            {
                descriptor.TagSize = 4;
            }
            return new UInt32Tag(descriptor, thisChannel, channel);
        }


        private Int64Tag CreateInt64Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
        {
            if (descriptor.TagSize == 0)
            {
                descriptor.TagSize = 8;
            }
            return new Int64Tag(descriptor, thisChannel, channel);
        }


        private UInt64Tag CreateUInt64Tag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
        {
            if (descriptor.TagSize == 0)
            {
                descriptor.TagSize = 8;
            }
            return new UInt64Tag(descriptor, thisChannel, channel);
        }


        private FloatTag CreateFloatTag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
        {
            if (descriptor.TagSize == 0)
            {
                descriptor.TagSize = 4;
            }
            return new FloatTag(descriptor, thisChannel, channel);
        }


        private StrTag CreateStrTag(TagDescriptor descriptor, ITagChannel? thisChannel, S7TagChannel channel)
        {
            S7Utils.NormalizeS7StrTagSize(descriptor, out byte maxlen);
            return new StrTag(descriptor, thisChannel, channel, maxLen: maxlen);
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
}
