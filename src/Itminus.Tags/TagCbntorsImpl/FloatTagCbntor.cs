using System.Buffers.Binary;

namespace Itminus.Tags;

public class FloatTagCbntor : TagCbntor
{

    public FloatTagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int cacheOffset)
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

            var ret = this.TagEndian() == EndianKinds.BigEndian ?
                BinaryPrimitives.ReadSingleBigEndian(span) :
                BinaryPrimitives.ReadSingleLittleEndian(span);

            return ret;
        }
        set
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            var data = (float)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.

            var dst = this.TagCbnt.Cache.Span.Slice(CacheOffset, 4);
            if (this.TagEndian() == EndianKinds.BigEndian)
            {
                BinaryPrimitives.WriteSingleBigEndian(dst, data);
            }
            else
            {
                BinaryPrimitives.WriteSingleLittleEndian(dst, data);
            }

            this.Timestamp = DateTime.Now;
            this.MarkDirty();
        }
    }

}