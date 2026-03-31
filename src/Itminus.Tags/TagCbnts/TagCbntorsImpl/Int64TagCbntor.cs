using System.Buffers.Binary;



namespace Itminus.Tags;

public class Int64TagCbntor : TagCbntor
{

    public Int64TagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int cacheOffset)
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
            var span = cache.Span.Slice(CacheOffset, 8);
            if (this.TagEndian() == EndianKinds.BigEndian)
            {
                return BinaryPrimitives.ReadInt64BigEndian(span);
            }
            return BinaryPrimitives.ReadInt64LittleEndian(span);
        }
        set
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            var data = (long)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.
            byte[] src = GetValueBytes(data);
            var dst = this.TagCbnt.Cache.Span.Slice(CacheOffset, 8);
            src.CopyTo(dst);

            Timestamp = DateTime.UtcNow;
            MarkDirty();
        }
    }

    private byte[] GetValueBytes(long data)
    {
        var src = BitConverter.GetBytes(data);
        if (this.TagEndian() == EndianKinds.BigEndian)
        {
            Array.Reverse(src);
        }
        return src;
    }


}
