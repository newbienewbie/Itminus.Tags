
namespace Itminus.Tags.SimpleFiles;

internal class SimpleFilesDirectTagFactory
{
    private readonly TagContainer _parent;

    public SimpleFilesDirectTagFactory(TagContainer parent)
    {
        _parent = parent;
    }

    private SimpleFilesTagChannel SearchRequiredChannel()
    {
        var channel = _parent.SearchRequiredChannel();
        if (channel is not SimpleFilesTagChannel simpleFilesChannel)
        {
            throw new Exception($"通道类型不匹配，期望={nameof(SimpleFilesTagChannel)}，实际={channel.GetType().Name}");
        }
        return simpleFilesChannel;
    }

    private BitDirectTag CreateBitTag(TagDescriptor descriptor, SimpleFilesTagChannel? thisChannel)
    {
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = sizeof(byte);
        }

        var channel = thisChannel ?? this.SearchRequiredChannel();
        descriptor.NormalizedAddress = channel.MakePath(descriptor.RawAddress);

        return new BitDirectTag(descriptor, thisChannel, _parent);
    }

    private ByteDirectTag CreateByteTag(TagDescriptor descriptor, SimpleFilesTagChannel? thisChannel)
    {
        if (descriptor.TagSize == 0)
        {
            descriptor.TagSize = sizeof(byte);
        }

        var channel = thisChannel ?? this.SearchRequiredChannel();
        descriptor.NormalizedAddress = channel.MakePath(descriptor.RawAddress);

        return new ByteDirectTag(descriptor, thisChannel, _parent);
    }


    public ITag Create(TagDescriptor descriptor, SimpleFilesTagChannel? thisChannel)
    {
        ITag tag = descriptor.TagKind switch
        {
            BuiltinTagKinds.BIT => CreateBitTag(descriptor, thisChannel),
            BuiltinTagKinds.BYTE => CreateByteTag(descriptor, thisChannel),
            _ => throw new Exception($"未预料到的测点种类={descriptor.TagKind}")
        };
        return tag;

    }

}
