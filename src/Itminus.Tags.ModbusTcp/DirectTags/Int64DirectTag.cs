
using System.Buffers.Binary;

namespace Itminus.Tags.ModbusTcp;

public class UInt64DirectTag : MutileBytesDirectTag<ulong>
{
    public UInt64DirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 8;

    protected override void FillBytes(ulong value, in Span<byte> buffer)
    {
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
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
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadUInt64BigEndian(bytes),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadUInt64LittleEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        }; 
    }
}


public class Int64DirectTag : MutileBytesDirectTag<long>
{
    public Int64DirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 8;

    protected override void FillBytes(long value, in Span<byte> buffer)
    {
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteInt64BigEndian(buffer, value);
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
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadInt64BigEndian(bytes),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadInt64LittleEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        };
    }
}
