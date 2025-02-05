using System.ComponentModel;
using Itminus.Tags;

namespace Itminus.Tags.ModbusTcp.Tags;


/// <summary>
/// ModBus的 DI 点，地址范围10000~19999
/// </summary>
public class DITag : TagCbntor
{

    /// <param name="tagDescriptor"></param>
    /// <param name="tagCbnt"></param>
    /// <param name="cacheOffset"></param>
    public DITag(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int cacheOffset)
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
        set => throw new Exception($"DI点({this.TagName}地址={this.TagAddress()})不可写入");
    }

}
