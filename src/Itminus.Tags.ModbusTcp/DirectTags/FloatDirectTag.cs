
using System.Buffers.Binary;

namespace Itminus.Tags.ModbusTcp;

internal class FloatDirectTag : MultipleBytesDirectTag<float>
{
    public FloatDirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 4;

    protected override void FillBytes(float value, in Span<byte> buffer)
    {
        // cache 固定每寄存器低字节在前。设备大端：先按大端写再逐寄存器交换成 cache 布局
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteSingleBigEndian(buffer, value);
                (buffer[0], buffer[1]) = (buffer[1], buffer[0]);
                (buffer[2], buffer[3]) = (buffer[3], buffer[2]);
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
        // cache 固定每寄存器低字节在前。设备大端：逐寄存器交换后按大端读
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadSingleBigEndian(SwapEachRegister(bytes)),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadSingleLittleEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        };
    }
}
