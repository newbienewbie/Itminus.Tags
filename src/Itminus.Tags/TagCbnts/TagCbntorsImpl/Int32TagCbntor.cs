using System.Buffers.Binary;

namespace Itminus.Tags;

public class Int32TagCbntor : TagCbntor
{

    public Int32TagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int cacheOffset)
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
                return BinaryPrimitives.ReadInt32BigEndian(span);
            }
            return BinaryPrimitives.ReadInt32LittleEndian(span);
        }
        set
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            var data = (Int32)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.
            byte[] src = GetValueBytes(data);
            var dst = this.TagCbnt.Cache.Span.Slice(CacheOffset, 4);
            src.CopyTo(dst);

            this.Timestamp = DateTime.UtcNow;
            this.MarkDirty();
        }
    }

    private byte[] GetValueBytes(Int32 data)
    {
        var src = BitConverter.GetBytes(data);
        if (this.TagEndian() == EndianKinds.BigEndian)
        {
            Array.Reverse(src);
        }
        return src;
    }


}
