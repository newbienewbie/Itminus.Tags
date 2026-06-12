using System.Buffers.Binary;

namespace Itminus.Tags.S7;

internal class Int16Tag : ContinousBytesBasedDirectTag<Int16>
{
    public Int16Tag(TagDescriptor descriptor, ITagChannel? thisChannel, IContinousBytesBasedTagChannel channel)
        : base(descriptor, thisChannel, channel)
    {
    }

    public override int BufferSize => 2;


    protected override short ConvertFromBytes(Span<byte> bytes)
    {
        return this.TagEndian() == EndianKinds.BigEndian ?
            BinaryPrimitives.ReadInt16BigEndian(bytes) :
            BinaryPrimitives.ReadInt16LittleEndian(bytes);
    }

    protected override void FillBytes(Span<byte> bytes, short value)
    {
        if (this.TagEndian() == EndianKinds.BigEndian)
        {
            BinaryPrimitives.WriteInt16BigEndian(bytes, this.Value);
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian(bytes, this.Value);
        }
    }
}
