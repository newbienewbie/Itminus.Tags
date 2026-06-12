using System.Buffers.Binary;

namespace Itminus.Tags.S7;

internal class Int64Tag : ContinousBytesBasedDirectTag<long>
{
    public Int64Tag(TagDescriptor descriptor, ITagChannel? thisChannel, IContinousBytesBasedTagChannel channel)
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
            BinaryPrimitives.WriteInt64BigEndian(bytes, this.Value);
        }
        else
        {
            BinaryPrimitives.WriteInt64LittleEndian(bytes, this.Value);
        }
    }
}
