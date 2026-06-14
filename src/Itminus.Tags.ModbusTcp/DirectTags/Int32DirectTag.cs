
using System.Buffers.Binary;

namespace Itminus.Tags.ModbusTcp;

public class UInt32DirectTag : MutileBytesDirectTag<Int32>
{
    public UInt32DirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 4;

    protected override void FillBytes(Int32 value, in Span<byte> buffer)
    {
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteInt32BigEndian(buffer, value);
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
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadInt32BigEndian(bytes),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadInt32LittleEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        }; 
    }
}


public class Int32DirectTag : MutileBytesDirectTag<int>
{
    public Int32DirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }

    protected override int BufferSize => 4;

    protected override void FillBytes(int value, in Span<byte> buffer)
    {
        switch (this.TagDescriptor.EndianKind)
        {
            case EndianKinds.BigEndian:
                BinaryPrimitives.WriteInt32BigEndian(buffer, value);
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
        return this.TagDescriptor.EndianKind switch
        {
            EndianKinds.BigEndian => BinaryPrimitives.ReadInt32BigEndian(bytes),
            EndianKinds.LittleEndian => BinaryPrimitives.ReadInt32LittleEndian(bytes),
            _ => throw new InvalidOperationException($"不支持的字节序类型: {this.TagDescriptor.EndianKind}")
        };
    }
}
