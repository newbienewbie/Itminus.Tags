namespace Itminus.Tags;

public class ByteTagCbntor : TagCbntor
{
    public ByteTagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagGroup, int cacheOffset) 
        : base(tagDescriptor, tagGroup, cacheOffset, cacheOffset)
    {
    }

    /// <summary>
    /// 测点值
    /// </summary>
    public override object? Value 
    { 
        get {
            var cache = this.TagCbnt.Cache;
            return cache.Span[this.CacheOffset];
        }
        set {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            var b = (byte)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.
            var cache = this.TagCbnt.Cache;
            cache.Span[this.CacheOffset] = b;

            this.Timestamp = DateTime.UtcNow;
            this.MarkDirty();
        } 
    }


}
