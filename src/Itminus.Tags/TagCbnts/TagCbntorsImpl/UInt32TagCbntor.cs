using System.Buffers.Binary;

namespace Itminus.Tags;

public class UInt32TagCbntor : TagCbntor
{

    public UInt32TagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int cacheOffset)
        : base(tagDescriptor, tagCbnt, cacheOffset, cacheOffset)
    {
    }

    /// <summary>
    /// 测点值
    /// </summary>
    public override object? Value
    {
        get
        {
            var cache = this.TagCbnt.Cache;
            var span = cache.Span.Slice(this.CacheOffset, 4);
            if (this.TagEndian() == EndianKinds.BigEndian)
            {
                return BinaryPrimitives.ReadUInt32BigEndian(span);
            }
            return BinaryPrimitives.ReadUInt32LittleEndian(span);
        }
        set
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            var data = (UInt32)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.
            byte[] src = GetValueBytes(data);
            var dst = this.TagCbnt.Cache.Span.Slice(CacheOffset, 4);
            src.CopyTo(dst);

            this.Timestamp = DateTime.UtcNow;
            this.MarkDirty();
        }
    }

    private byte[] GetValueBytes(UInt32 data)
    {
        var src = BitConverter.GetBytes(data);
        if (this.TagEndian() == EndianKinds.BigEndian)
        {
            Array.Reverse(src);
        }
        return src;
    }


}