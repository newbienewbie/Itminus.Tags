using System.Buffers.Binary;

namespace Itminus.Tags.DirectTags;

internal class Int32DirectTag : ContinousBytesBasedDirectTag<Int32>
{
    public Int32DirectTag(TagDescriptor descriptor, ITagChannel? thisChannel, TagContainer parent)
        : base(descriptor, thisChannel, parent)
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
            BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        }
    }
}
