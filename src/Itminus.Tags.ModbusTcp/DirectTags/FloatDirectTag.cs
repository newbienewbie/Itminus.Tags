
using System.Buffers.Binary;

namespace Itminus.Tags.ModbusTcp;

internal class FloatDirectTag : MutipleBytesDirectTag<float>
{
    public FloatDirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 4;

    protected override void FillBytes(float value, in Span<byte> buffer)
    {
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteSingleBigEndian(buffer, value);
                break;
            case EndianKinds.LittleEndian:
                BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
                break;
            default:
                throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}");
        }
    }

    protected override float GetValueFromBytes(byte[] bytes)
    {
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadSingleBigEndian(bytes),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadSingleLittleEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        };
    }
}
