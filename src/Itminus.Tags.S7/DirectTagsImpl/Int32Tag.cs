using System.Buffers.Binary;

namespace Itminus.Tags.S7;

internal class Int32Tag : ContinousBytesBasedDirectTag<Int32>
{
    public Int32Tag(TagDescriptor descriptor, ITagChannel? thisChannel, IContinousBytesBasedTagChannel channel)
        : base(descriptor, thisChannel, channel)
    {
    }

    public override int BufferSize => 4;


    protected override int ConvertFromBytes(Span<byte> bytes)
    {
        return this.TagEndian() == EndianKinds.BigEndian ?
            BinaryPrimitives.ReadInt32BigEndian(bytes) :
            BinaryPrimitives.ReadInt32LittleEndian(bytes);
    }

    protected override void FillBytes(Span<byte> bytes, int value)
    {
        if (this.TagEndian() == EndianKinds.BigEndian)
        {
            BinaryPrimitives.WriteInt32BigEndian(bytes, this.Value);
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian(bytes, this.Value);
        }
    }
}
