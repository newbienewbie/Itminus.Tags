using System.Buffers.Binary;

namespace Itminus.Tags.TagCbntors;



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
            var data = (Int16)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.

            var dst = this.TagCbnt.Cache.Span.Slice(CacheOffset,2);
            if (this.TagEndian() == EndianKinds.BigEndian)
            {
                BinaryPrimitives.WriteInt16BigEndian(dst, data);
            }
            else
            {
                BinaryPrimitives.WriteInt16LittleEndian(dst, data);
            }

            this.Timestamp = DateTime.Now;
            this.MarkDirty();
        }
    }

}
