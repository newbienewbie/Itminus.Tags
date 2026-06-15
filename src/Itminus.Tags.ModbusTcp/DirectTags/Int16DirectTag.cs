
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
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
                break;
            case EndianKinds.LittleEndian:
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
                break;
            default:
                throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}");
        }
    }

    protected override ushort GetValueFromBytes(byte[] bytes)
    {
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadUInt16BigEndian(bytes),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadUInt16LittleEndian(bytes),
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
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteInt16BigEndian(buffer, value);
                break;
            case EndianKinds.LittleEndian:
                BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
                break;
            default:
                throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}");
        }
    }

    protected override short GetValueFromBytes(byte[] bytes)
    {
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadInt16BigEndian(bytes),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadInt16LittleEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        };
    }
}
