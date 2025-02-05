using System.Xml.Linq;

namespace Itminus.Tags.Projects;

public static class XElementExensions
{
    #region helpers
    public static string GetTagUnionName(this XElement e)
    {
        var tagName = (string?)e.Attribute("name") ?? throw new Exception($"Tag 未配置名称");
        return tagName;
    }

    public static bool GetTagUnionIsEntry(this XElement e, string tagName)
    {
        var isEntry = (bool?)e.Attribute("isEntry") ?? false;
        return isEntry;
    }

    public static string GetTagUnionAddress(this XElement e, string tagName)
    {
        var address = (string?)e.Attribute("address") ?? throw new Exception($"Tag(Name={tagName})未配置地址");
        return address;
    }

    public static ITagChannel? GetTagUnionChannel(this XElement e, IList<ITagChannel> channels)
    {
        var channelName = (string?)e.Attribute("channel");
        var channel = string.IsNullOrEmpty(channelName) ?
            null :
            channels.FirstOrDefault(c => c.ChannelName == channelName);
        return channel;
    }

    public static TagKinds GetTagUnionTagKind(this XElement e, string tagName)
    {
        var type = (string?)e.Attribute("type") ?? throw new Exception($"Tag(Name={tagName})未配置类型");
        if (!Enum.TryParse<TagKinds>(type, out var tagKind))
        {
            throw new Exception($"Tag(Name={tagName}) 配置了未知类型={type}");
        }
        return tagKind;
    }

    public static EndianKinds GetTagUnionEndian(this XElement e, string tagName)
    {
        var type = (string?)e.Attribute("endian");
        if(string.IsNullOrEmpty(type))
        {
            return EndianKinds.LittleEndian;
        }    
        if (!Enum.TryParse<EndianKinds>(type, out var endian))
        {
            throw new Exception($"Tag(Name={tagName}) 配置了未知的字节序={type}");
        }
        return endian;
    }


    public static TagAccessMode? GetTagUnionAccess(this XElement e, string tagName)
    {
        var modestr = (string?)e.Attribute("access");
        if (string.IsNullOrEmpty(modestr))
        {
            return null;
        }
        if (!Enum.TryParse<TagAccessMode>(modestr, out var access))
        {
            throw new Exception($"Tag(Name={tagName}) 配置了未知的访问模式={modestr}");
        }
        return access;
    }

    public static string? GetTagUnionNote(this XElement e, string tagName)
    {
        var note = (string?)e.Attribute("note");
        return note;
    }

    public static string? GetTagUnionDriver(this XElement e, string tagName)
    {
        var note = (string?)e.Attribute("driver");
        return note;
    }


    public static T MapTagUnion<T>(this XElement e, Func<XElement,T> mapTag, Func<XElement,T> mapTagCbnt, Func<XElement,T> mapTagGrp)
    {
        if (e.Name == "Tag")
        {
            return mapTag(e);
        }
        else if (e.Name == "TagCbnt")
        {
            return mapTagCbnt(e);
        }
        else if (e.Name == "TagGrp")
        {
            return mapTagGrp(e);
        }
        else
        {
            throw new Exception($"未知的测点配置元素:<{e.Name}/>");
        }
    }
    #endregion
}


