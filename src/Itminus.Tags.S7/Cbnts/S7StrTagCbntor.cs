using System.Buffers.Binary;
using System.Text;



namespace Itminus.Tags.S7;

public class S7StrTagCbntor : TagCbntor
{
    /// <summary>
    /// 字符串最大长度，ReadOnly
    /// </summary>
    public byte Maxlen { get; }

    /// <summary>
    /// 字符串有效长度，ReadOnly。
    /// </summary>
    public byte Strlen
    {
        get
        {
            var cache = this.TagCbnt.Cache;
            var span = cache.Span.Slice(CacheOffset);
            var total = span[0];
            var size = span[1];
            return size;
        }
    }

    internal S7StrTagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int cacheOffset, byte maxLen)
        : base(tagDescriptor, tagCbnt, cacheOffset, cacheOffset)
    {
        this.Maxlen = maxLen;
    }

    /// <summary>
    /// 测点值
    /// </summary>
    public override object? Value
    {
        get
        {
            var cache = this.TagCbnt.Cache;
            var span = cache.Span.Slice(CacheOffset);
            var total = span[0];
            var size = span[1];
            var str = Encoding.ASCII.GetString(span.Slice(2, size));
            return str;
        }
        set
        {
            var str = value is null ? string.Empty : value is string s ? s : throw new InvalidCastException();
            var cache = this.TagCbnt.Cache;
            var tagsize = this.TagSize();
            var span = cache.Span.Slice(CacheOffset, tagsize);
            var total = span[0];
            var size = span[1];
            if(str.Length > total)
            {
                throw new ArgumentException($"字符串长度超过限制，最大{total}，实际{str.Length}");
            }

            var read = Encoding.ASCII.GetBytes(str, span.Slice(2));
            span[1] = (byte)read;

            Timestamp = DateTime.UtcNow;
            MarkDirty();
        }
    }

}