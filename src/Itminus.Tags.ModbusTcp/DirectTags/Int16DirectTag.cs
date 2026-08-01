
using System.Buffers.Binary;

namespace Itminus.Tags.ModbusTcp;

internal class UInt16DirectTag : MultipleBytesDirectTag<ushort>
{
    public UInt16DirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 2;

    protected override void FillBytes(ushort value, in Span<byte> buffer)
    {
        // 16 位：cache 是"读回数值的小端内存形态"（见 MultipleBytesDirectTag 注释）。
        // 写回时按 cache 形态写：设备 BigEndian → Write*LittleEndian（负负得正）；设备 LittleEndian → Write*BigEndian
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
                break;
            case EndianKinds.LittleEndian:
                BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
                break;
            default:
                throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}");
        }
    }

    protected override ushort GetValueFromBytes(byte[] bytes)
    {
        // 16 位：cache = 读回数值的小端内存形态。设备 BigEndian（读回=物理值）用小端读还原；设备 LittleEndian 用大端读
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadUInt16LittleEndian(bytes),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadUInt16BigEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        }; 
    }
}


internal class Int16DirectTag : MultipleBytesDirectTag<short>
{
    public Int16DirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 2;

    protected override void FillBytes(short value, in Span<byte> buffer)
    {
        // 16 位：cache 是"读回数值的小端内存形态"（见 MultipleBytesDirectTag 注释）。
        // 写回时按 cache 形态写：设备 BigEndian → Write*LittleEndian（负负得正）；设备 LittleEndian → Write*BigEndian
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
                break;
            case EndianKinds.LittleEndian:
                BinaryPrimitives.WriteInt16BigEndian(buffer, value);
                break;
            default:
                throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}");
        }
    }

    protected override short GetValueFromBytes(byte[] bytes)
    {
        // 16 位：cache = 读回数值的小端内存形态。设备 BigEndian 用小端读还原；设备 LittleEndian 用大端读
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadInt16LittleEndian(bytes),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadInt16BigEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        };
    }
}
