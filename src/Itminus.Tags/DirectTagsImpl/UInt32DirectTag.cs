using System.Buffers.Binary;

namespace Itminus.Tags.DirectTags;

internal class UInt32DirectTag : ContinousBytesBasedDirectTag<UInt32>
{
    public UInt32DirectTag(TagDescriptor descriptor, ITagChannel? thisChannel, IContinousBytesBasedTagChannel channel)
        : base(descriptor, thisChannel, channel)
    {
    }

    public override int BufferSize => 4;


    protected override uint ConvertFromBytes(Span<byte> bytes)
    {
        return this.TagEndian() == EndianKinds.BigEndian ?
            BinaryPrimitives.ReadUInt32BigEndian(bytes) :
            BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    }

    protected override void FillBytes(Span<byte> bytes, uint value)
    {
        if (this.TagEndian() == EndianKinds.BigEndian)
        {
            BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        }
    }
}
