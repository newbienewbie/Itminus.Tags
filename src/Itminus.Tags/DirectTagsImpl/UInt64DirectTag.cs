using System.Buffers.Binary;

namespace Itminus.Tags.DirectTags;

internal class UInt64DirectTag : ContinousBytesBasedDirectTag<ulong>
{
    public UInt64DirectTag(TagDescriptor descriptor, ITagChannel? thisChannel, TagContainer container)
        : base(descriptor,thisChannel, container)
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
