using System.Xml.Linq;

namespace Itminus.Tags;

/// <summary>
/// 测点组合描述符
/// </summary>
public class TagCbntDescriptor : ITagsDescriptor
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 通道名
    /// </summary>
    public string? ChannelName { get; set; }

    /// <summary>
    /// 起始地址
    /// </summary>
    public string StartAddress { get; set; } = "";

    /// <summary>
    /// 扫描间隔
    /// </summary>
    public int ScanInterval { get; set; } = 200;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 访问模式
    /// </summary>
    public TagAccessMode AccessMode { get; set; } = TagAccessMode.RW;

    /// <summary>
    /// 额外特性
    /// </summary>
    public IDictionary<string, XAttribute> Extras { get; } = new Dictionary<string, XAttribute>();

    /// <summary>
    /// 子节点描述
    /// </summary>
    public IList<TagDescriptor> Children { get; } = new List<TagDescriptor>();
}

