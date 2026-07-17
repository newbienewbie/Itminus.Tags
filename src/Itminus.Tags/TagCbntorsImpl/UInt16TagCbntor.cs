using System.Buffers.Binary;

namespace Itminus.Tags.TagCbntors;

/// <summary>
/// 表示一个 uint16 类型的<see cref="TagCbntor"/>
/// </summary>
public class UInt16TagCbntor : TagCbntor
{
    /// <summary>
    /// c'tor
    /// </summary>
    public UInt16TagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int cacheOffset)
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
            var span = cache.Span.Slice(this.CacheOffset, 2);
            var ret = this.TagEndian() == EndianKinds.BigEndian ?
                BinaryPrimitives.ReadUInt16BigEndian(span) :
                BinaryPrimitives.ReadUInt16LittleEndian(span);
            return ret;
        }
        set
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            var data = (UInt16)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.

            var dst = this.TagCbnt.Cache.Span.Slice(CacheOffset, 2);
            if (this.TagEndian() == EndianKinds.BigEndian)
            {
                BinaryPrimitives.WriteUInt16BigEndian(dst, data);
            }
            else
            {
                BinaryPrimitives.WriteUInt16LittleEndian(dst, data);
            }

            this.Timestamp = DateTime.Now;
            this.MarkDirty();
        }
    }

}
