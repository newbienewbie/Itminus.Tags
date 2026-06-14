using System.Buffers.Binary;

namespace Itminus.Tags.DirectTags;


internal class FloatDirectTag : ContinousBytesBasedDirectTag<float>
{
    public FloatDirectTag(TagDescriptor descriptor, ITagChannel? thisChannel, TagContainer parent)
        : base(descriptor, thisChannel, parent)
    {
    }

    public override int BufferSize => 4;

    protected override float ConvertFromBytes(Span<byte> bytes)
    {
        return this.TagEndian() == EndianKinds.BigEndian ?
            BinaryPrimitives.ReadSingleBigEndian(bytes) :
            BinaryPrimitives.ReadSingleLittleEndian(bytes);
    }

    protected override void FillBytes(Span<byte> bytes, float value)
    {
        if (this.TagEndian() == EndianKinds.BigEndian)
        {
            BinaryPrimitives.WriteSingleBigEndian(bytes, value);
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian(bytes, value);
        }
    }
}