using System.Xml.Linq;

namespace Itminus.Tags;

public static class XElementExtensions_TagDescriptor
{
    #region
    /// <summary>
    /// 构造 <see cref="TagDescriptor"/> 对象
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static TagDescriptor ToTagDescriptor(this XElement e)
    {
        var tagName = e.GetTagUnionName();
        var address = e.GetTagUnionAddress(tagName);
        var tagKind = e.GetTagUnionTagKind(tagName);
        var tagEndian = e.GetTagUnionEndian(tagName);
        var tagChannelName = e.GetTagUnionChannelName();
        var tagNote = e.GetTagUnionNote(tagName);

        var tagdescriptor = new TagDescriptor()
        {
            Address = address,
            TagName = tagName,
            TagKind = tagKind,
            EndianKind = tagEndian,
            ChannelName = tagChannelName,
            Note = tagNote,
        };

        var tagAccess = e.GetTagUnionAccess(tagName);
        if (tagAccess.HasValue)
        {
            tagdescriptor.AccessMode = tagAccess.Value;
        }

        var tagSize = (string?)e.Attribute("tagSize");
        if (!string.IsNullOrEmpty(tagSize))
        {
            if (!int.TryParse(tagSize, out var size))
            {
                throw new Exception($"Tag(Name={tagName}) 配置了非法大小={tagSize}");
            }
            tagdescriptor.TagSize = size;
        }
        return tagdescriptor;
    }

    /// <summary>
    /// 构造 XElement
    /// </summary>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public static XElement ToXElement(this TagDescriptor descriptor)
    {
        var name = new XAttribute("name", descriptor.TagName);
        var address = new XAttribute("address", descriptor.Address);
        var tagEndian = new XAttribute("endian", descriptor.EndianKind);
        var access = new XAttribute("access", descriptor.AccessMode);


        var attrs = new List<XAttribute> {
            name,
            address,
            tagEndian,
            access,
        };
        if (descriptor.TagKind != BuiltinTagKinds.Unknown)
        {
            var tagKind = new XAttribute("type", descriptor.TagKind);
            attrs.Add(tagKind);
        }
        if (descriptor.TagSize != default)
        {
            var tagSize = new XAttribute("tagSize", descriptor.TagSize);
            attrs.Add(tagSize);
        }
        if (!string.IsNullOrEmpty(descriptor.ChannelName))
        {
            attrs.Add(new XAttribute("channel", descriptor.ChannelName));
        }

        if (!string.IsNullOrEmpty(descriptor.Note))
        {
            attrs.Add(new XAttribute("note", descriptor.Note));
        }
        var ele = new XElement("Tag", attrs);
        return ele;
    }

    #endregion
}
