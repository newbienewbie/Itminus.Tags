using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Itminus.Tags;


/// <summary>
/// <see cref="ITagChannel"/> 描述符
/// </summary>
public class TagChannelDescriptor
{

    /// <summary>
    /// Channel Name
    /// </summary>
    public string Name { get; set; } = string.Empty;


    /// <summary>
    /// Channel Driver
    /// </summary>
    public virtual string Driver { get; set; } = string.Empty;

    /// <summary>
    /// 额外参数
    /// </summary>
    public IDictionary<string, XElement> Extras { get; set; } = new Dictionary<string, XElement>();


    public static TagChannelDescriptor LoadFromXElement(XElement e)
    {
        var name = e.Attribute("name")?.Value ?? throw new Exception($"通道元素未配置元素名({e.Name.LocalName})");
        var driver = e.Attribute("driver")?.Value ?? throw new Exception($"通道元素未配置驱动({e.Name.LocalName})"); ;

        var descriptor = new TagChannelDescriptor()
        {
            Name = name,
            Driver = driver,
            Extras = e.Elements()
                .ToDictionary(child => child.Name.LocalName, child => child)
        };
        return descriptor;
    }
}

