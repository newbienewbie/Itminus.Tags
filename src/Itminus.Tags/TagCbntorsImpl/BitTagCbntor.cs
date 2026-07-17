namespace Itminus.Tags.TagCbntors;

/// <summary>
/// 表示单个比特型的 TagCbntor
/// </summary>
public class BitTagCbntor : TagCbntor
{
    /// <summary>
    /// c'tor
    /// </summary>
    public BitTagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int tagOffset, int cacheOffset, byte nthBit)
        : base(tagDescriptor, tagCbnt, tagOffset, cacheOffset)
    {
        this.NthBit = nthBit;
    }

    /// <summary>
    /// 第Nth位比特: 取值范围 0~7。
    /// 注意，尽管测点经常会表示成0~15之间的位地址，这里都会被统一规整化到 0~7。
    /// 当 8~15 被规则化到 0~7，会导致 tagOffset 与 cacheOffset 相差1。
    /// 如果要知道用户层面指定的“视地址”，可以使用 tag.TagAddress(ITag) 
    /// </summary>
    public byte NthBit { get; set; }

    /// <summary>
    /// 测点值
    /// </summary>
    public override object? Value 
    { 
        get 
        {
            var cache = this.TagCbnt.Cache;
            var flags = cache.Span[this.CacheOffset];
            var hasFlag = flags & (1 << this.NthBit);
            return hasFlag != 0;
        }
        set {
            if (value is not bool b)
            {
                throw new Exception($"不应该为Bit类型的测点赋值一个类型为{value?.GetType().Name}值");
            }
            var cache = this.TagCbnt.Cache;
            var oldFlags = cache.Span[this.CacheOffset];
            var newFlags = b ?
                oldFlags | 1 << this.NthBit :
                oldFlags & ~(1 << this.NthBit);
            cache.Span[this.CacheOffset] = (byte) newFlags;
            this.Timestamp = DateTime.Now;
            this.MarkDirty();
        } 
    }


}
