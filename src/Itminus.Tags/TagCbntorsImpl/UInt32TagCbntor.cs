using System.Buffers.Binary;

namespace Itminus.Tags.TagCbntors;


/// <summary>
/// 表示一个 uint32 类型的<see cref="TagCbntor"/>
/// </summary>
public class UInt32TagCbntor : TagCbntor
{
    /// <summary>
    /// c'tor
    /// </summary>
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

            var dst = this.TagCbnt.Cache.Span.Slice(CacheOffset, 4);
            if (this.TagEndian() == EndianKinds.BigEndian)
            {
                BinaryPrimitives.WriteUInt32BigEndian(dst, data);
            }
            else
            {
                BinaryPrimitives.WriteUInt32LittleEndian(dst, data);
            }

            this.Timestamp = DateTime.Now;
            this.MarkDirty();
        }
    }

}