namespace Itminus.Tags.ModbusTcp.Tags;


/// <summary>
/// Modbus的DO点，地址范围00000~09999
/// </summary>
public class DOTag : TagCbntor
{

    /// <param name="tagDescriptor"></param>
    /// <param name="tagCbnt"></param>
    /// <param name="cacheOffset"></param>
    public DOTag(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int cacheOffset)
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
            var cache = TagCbnt.Cache;
            var flags = cache.Span[CacheOffset];
            return flags != 0;
        }
        set
        {
            if (value is not bool b)
            {
                throw new Exception($"不应该为Bit类型的测点赋值一个类型为{value?.GetType().Name}值");
            }

            if (Value != null && !Value.Equals(b))
            {
                var cache = TagCbnt.Cache;
                cache.Span[CacheOffset] = b ? (byte)1 : (byte)0;
            }


            Timestamp = DateTime.UtcNow;
            MarkDirty();
        }
    }

}