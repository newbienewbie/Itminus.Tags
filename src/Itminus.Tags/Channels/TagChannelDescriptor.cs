using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Itminus.Tags;


/// <summary>
/// <see cref="ITagChannel"/> 描述符
/// </summary>
public class TagChannelDescriptor
{

    /// <summary>
    /// Channel Name
    /// </summary>
    [XmlAttribute("name")]
    public virtual string Name { get; set; } = string.Empty;


    /// <summary>
    /// Channel Driver
    /// </summary>
    [XmlAttribute("driver")]
    public virtual string Driver { get; set; } = string.Empty;

    /// <summary>
    /// 额外参数
    /// </summary>
    public virtual IDictionary<string, XElement> Extras { get; set; } = new Dictionary<string, XElement>();

    /// <summary>
    /// 从 XElement 加载
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static TagChannelDescriptor LoadFromXElement(XElement e)
    {
        var name = e.Attribute("name")?.Value ?? throw new Exception($"通道元素未配置元素名({e.Name.LocalName})");
        var driver = e.Attribute("driver")?.Value ?? throw new Exception($"通道元素未配置驱动({e.Name.LocalName})"); ;

        var descriptor = new TagChannelDescriptor()
        {
            Name = name,
            Driver = driver,
            Extras = e.Elements().ToDictionary(child => child.Name.LocalName, child => child)
        };
        return descriptor;
    }

    public virtual TagChannelDescriptor Copy()
    {
        var xml = this.ToXElement();
        var descriptor = LoadFromXElement(xml);
        return descriptor;
    }

    /// <summary>
    /// 转行XElement
    /// </summary>
    /// <returns></returns>
    public virtual XElement ToXElement()
    {
        var element = new XElement("Channel",
            new XAttribute("name", Name),
            new XAttribute("driver", Driver)
        );

        foreach (var kvp in Extras)
        {
            element.Add(kvp.Value);
        }

        return element;
    }


}

