
using System.Buffers.Binary;

namespace Itminus.Tags.ModbusTcp;

internal class UInt64DirectTag : MultipleBytesDirectTag<ulong>
{
    public UInt64DirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 8;

    protected override void FillBytes(ulong value, in Span<byte> buffer)
    {
        // cache 固定每寄存器低字节在前。设备大端：先按大端写再逐寄存器交换成 cache 布局
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
                for (int i = 0; i + 1 < buffer.Length; i += 2) (buffer[i], buffer[i + 1]) = (buffer[i + 1], buffer[i]);
                break;
            case EndianKinds.LittleEndian:
                BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
                break;
            default:
                throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}");
        }
    }

    protected override ulong GetValueFromBytes(byte[] bytes)
    {
        // cache 固定每寄存器低字节在前。设备大端：逐寄存器交换后按大端读
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadUInt64BigEndian(SwapEachRegister(bytes)),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadUInt64LittleEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        }; 
    }
}


internal class Int64DirectTag : MultipleBytesDirectTag<long>
{
    public Int64DirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 8;

    protected override void FillBytes(long value, in Span<byte> buffer)
    {
        // cache 固定每寄存器低字节在前。设备大端：先按大端写再逐寄存器交换成 cache 布局
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteInt64BigEndian(buffer, value);
                for (int i = 0; i + 1 < buffer.Length; i += 2) (buffer[i], buffer[i + 1]) = (buffer[i + 1], buffer[i]);
                break;
            case EndianKinds.LittleEndian:
                BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
                break;
            default:
                throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}");
        }
    }

    protected override long GetValueFromBytes(byte[] bytes)
    {
        // cache 固定每寄存器低字节在前。设备大端：逐寄存器交换后按大端读
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadInt64BigEndian(SwapEachRegister(bytes)),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadInt64LittleEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        };
    }
}
