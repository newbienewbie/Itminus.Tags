using System.Buffers.Binary;


namespace Itminus.Tags.TagCbntors;

/// <summary>
/// 表示一个 uint64 类型的<see cref="TagCbntor"/>
/// </summary>
public class UInt64TagCbntor : TagCbntor
{
    /// <summary>
    /// c'tor
    /// </summary>
    public UInt64TagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int cacheOffset)
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
                return BinaryPrimitives.ReadUInt64BigEndian(span);
            }
            return BinaryPrimitives.ReadUInt64LittleEndian(span);
        }
        set
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            var data = (ulong)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.

            var dst = this.TagCbnt.Cache.Span.Slice(CacheOffset, 8);
            if (this.TagEndian() == EndianKinds.BigEndian)
            {
                BinaryPrimitives.WriteUInt64BigEndian(dst, data);
            }
            else
            {
                BinaryPrimitives.WriteUInt64LittleEndian(dst, data);
            }

            Timestamp = DateTime.Now;
            MarkDirty();
        }
    }

}