using System;

namespace Itminus.Tags.DirectTags;


internal class ByteDirectTag : ContinousBytesBasedDirectTag<byte>
{
    public ByteDirectTag(TagDescriptor descriptor, ITagChannel? thisChannel, IContinousBytesBasedTagChannel channel) 
        : base(descriptor, thisChannel, channel)
    {
    }

    public override int BufferSize => 1;

    protected override byte ConvertFromBytes(Span<byte> bytes) => bytes[0];

    protected override void FillBytes(Span<byte> bytes, byte value)
    {
        bytes[0] = value;
    }
}



