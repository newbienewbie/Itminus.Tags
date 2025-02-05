using System.Buffers.Binary;

namespace Itminus.Tags;


public class Int16TagCbntor : TagCbntor
{

    public Int16TagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int cacheOffset) 
        : base(tagDescriptor, tagCbnt, cacheOffset, cacheOffset)
    {
    }

    /// <summary>
    /// 测点值
    /// </summary>
    public override object? Value 
    { 
        get {
            var cache = this.TagCbnt.Cache;
            var span = cache.Span.Slice(this.CacheOffset,2);
            var ret = this.TagEndian() == EndianKinds.BigEndian ?
                BinaryPrimitives.ReadInt16BigEndian(span) : 
                BinaryPrimitives.ReadInt16LittleEndian(span);
            return ret;
        }
        set
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            var data = (UInt16)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.
            byte[] src = GetUShortValueBytes(data);
            var dst = this.TagCbnt.Cache.Span.Slice(CacheOffset, 2);
            src.CopyTo(dst);

            this.Timestamp = DateTime.UtcNow;
            this.MarkDirty();
        }
    }

    private byte[] GetUShortValueBytes(ushort data)
    {
        var src = BitConverter.GetBytes(data);
        if (this.TagEndian() == EndianKinds.BigEndian)
        {
            Array.Reverse(src);
        }
        return src;
    }


}
