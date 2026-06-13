using System.Buffers.Binary;

namespace Itminus.Tags.S7;

internal class UInt64Tag : ContinousBytesBasedDirectTag<ulong>
{
    public UInt64Tag(TagDescriptor descriptor, ITagChannel? thisChannel, IContinousBytesBasedTagChannel channel)
        : base(descriptor,thisChannel, channel)
    {
    }

    public override int BufferSize => 8;

    protected override ulong ConvertFromBytes(Span<byte> bytes)
    {
        return this.TagEndian() == EndianKinds.BigEndian ?
            BinaryPrimitives.ReadUInt64BigEndian(bytes) :
            BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }

    protected override void FillBytes(Span<byte> bytes, ulong value)
    {
        if (this.TagEndian() == EndianKinds.BigEndian)
        {
            BinaryPrimitives.WriteUInt64BigEndian(bytes, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        }
    }
}
