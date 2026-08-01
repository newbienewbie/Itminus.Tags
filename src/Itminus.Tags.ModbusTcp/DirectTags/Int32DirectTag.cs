
using System.Buffers.Binary;

namespace Itminus.Tags.ModbusTcp;

internal class UInt32DirectTag : MultipleBytesDirectTag<uint>
{
    public UInt32DirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 4;

    protected override void FillBytes(uint value, in Span<byte> buffer)
    {
        // cache 固定每寄存器低字节在前。设备大端：高寄存器在前，先按大端写再逐寄存器交换成 cache 布局
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
                (buffer[0], buffer[1]) = (buffer[1], buffer[0]);
                (buffer[2], buffer[3]) = (buffer[3], buffer[2]);
                break;
            case EndianKinds.LittleEndian:
                BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
                break;
            default:
                throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}");
        }
    }

    protected override uint GetValueFromBytes(byte[] bytes)
    {
        // cache 固定每寄存器低字节在前。设备大端：高寄存器在前，逐寄存器交换后按大端读
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadUInt32BigEndian(SwapEachRegister(bytes)),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadUInt32LittleEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        }; 
    }
}


internal class Int32DirectTag : MultipleBytesDirectTag<int>
{
    public Int32DirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 4;

    protected override void FillBytes(int value, in Span<byte> buffer)
    {
        // cache 固定每寄存器低字节在前。设备大端：先按大端写再逐寄存器交换成 cache 布局
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteInt32BigEndian(buffer, value);
                (buffer[0], buffer[1]) = (buffer[1], buffer[0]);
                (buffer[2], buffer[3]) = (buffer[3], buffer[2]);
                break;
            case EndianKinds.LittleEndian:
                BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
                break;
            default:
                throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}");
        }
    }

    protected override int GetValueFromBytes(byte[] bytes)
    {
        // cache 固定每寄存器低字节在前。设备大端：逐寄存器交换后按大端读
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadInt32BigEndian(SwapEachRegister(bytes)),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadInt32LittleEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        };
    }
}
