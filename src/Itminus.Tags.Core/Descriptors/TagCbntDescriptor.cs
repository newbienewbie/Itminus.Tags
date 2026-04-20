using System.Xml.Linq;

namespace Itminus.Tags;

/// <summary>
/// 测点组合描述符
/// </summary>
public class TagCbntDescriptor : ITagsDescriptor
{
    public string Name { get; set; } = "";
    public string? ChannelName { get; set; }
    public string StartAddress { get; set; } = "";

    public int ScanInterval { get; set; } = 200;
    public bool IsEnabled { get; set; } = true;

    public TagAccessMode AccessMode { get; set; } = TagAccessMode.RW;

    /// <summary>
    /// 额外特性
    /// </summary>
    public IDictionary<string, XAttribute> Extras { get; } = new Dictionary<string, XAttribute>();

    public IList<TagDescriptor> Children { get; } = new List<TagDescriptor>();
}

