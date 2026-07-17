using System.Xml.Linq;

namespace Itminus.Tags;

/// <summary>
/// TagGrp 描述符
/// </summary>
public class TagGrpDescriptor: ITagsDescriptor
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 通道
    /// </summary>
    public string? ChannelName { get; set; }

    /// <summary>
    /// 是否入口?
    /// </summary>
    public bool IsEntry { get; set; } = false;

    /// <summary>
    /// 是否启用？
    /// </summary>
    public bool IsEnabled { get; set; } = true; 

    /// <summary>
    /// 扫描间隔
    /// </summary>
    public int ScanInterval { get; set; } = 200;

    /// <summary>
    /// 额外属性
    /// </summary>
    public IDictionary<string, XAttribute> Extras { get; } = new Dictionary<string, XAttribute>();

    /// <summary>
    /// 子节点描述
    /// </summary>
    public IList<ITagsDescriptor> Children { get; } = new List<ITagsDescriptor>();
}