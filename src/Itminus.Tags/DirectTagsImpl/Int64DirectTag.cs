using System.Buffers.Binary;

namespace Itminus.Tags.DirectTags;

internal class Int64DirectTag : ContinousBytesBasedDirectTag<long>
{
    public Int64DirectTag(TagDescriptor descriptor, ITagChannel? thisChannel, IContinousBytesBasedTagChannel channel)
        : base(descriptor, thisChannel, channel)
    {
    }

    public override int BufferSize => 8;

    protected override long ConvertFromBytes(Span<byte> bytes)
    {
        return this.TagEndian() == EndianKinds.BigEndian ?
            BinaryPrimitives.ReadInt64BigEndian(bytes) :
            BinaryPrimitives.ReadInt64LittleEndian(bytes);
    }

    protected override void FillBytes(Span<byte> bytes, long value)
    {
        if (this.TagEndian() == EndianKinds.BigEndian)
        {
            BinaryPrimitives.WriteInt64BigEndian(bytes, value);
        }
        else
        {
            BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
        }
    }
}
