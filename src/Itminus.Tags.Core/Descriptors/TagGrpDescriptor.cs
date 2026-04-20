using System.Xml.Linq;

namespace Itminus.Tags;

public class TagGrpDescriptor: ITagsDescriptor
{
    public string Name { get; set; } = "";
    public string? ChannelName { get; set; }

    public bool IsEntry { get; set; } = false;
    public bool IsEnabled { get; set; } = true; 

    public int ScanInterval { get; set; } = 200;

    /// <summary>
    /// 额外属性
    /// </summary>
    public IDictionary<string, XAttribute> Extras { get; } = new Dictionary<string, XAttribute>();

    public IList<ITagsDescriptor> Children { get; } = new List<ITagsDescriptor>();
}