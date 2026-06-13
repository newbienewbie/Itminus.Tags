using System.Buffers.Binary;

namespace Itminus.Tags.S7;

internal class UInt16Tag : ContinousBytesBasedDirectTag<UInt16>
{
    public UInt16Tag(TagDescriptor descriptor, ITagChannel? thisChannel, IContinousBytesBasedTagChannel channel)
        : base(descriptor,thisChannel, channel)
    {
    }

    public override int BufferSize => 2;


    protected override ushort ConvertFromBytes(Span<byte> bytes)
    {
        return this.TagEndian() == EndianKinds.BigEndian ?
            BinaryPrimitives.ReadUInt16BigEndian(bytes) :
            BinaryPrimitives.ReadUInt16LittleEndian(bytes);
    }

    protected override void FillBytes(Span<byte> bytes, ushort value)
    {
        if (this.TagEndian() == EndianKinds.BigEndian)
        {
            BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt16LittleEndian(bytes, value);
        }
    }
}