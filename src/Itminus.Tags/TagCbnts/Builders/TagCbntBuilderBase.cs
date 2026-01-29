
using Itminus.Tags.Projects;
using System.Net;
using System.Threading.Channels;
using System.Xml.Linq;

namespace Itminus.Tags;


/// <summary>
/// 测点组合的构建器基类，用于构建一个测点组合。<br/>
/// 子类必须提供一个无参构造函数
/// </summary>
public abstract class TagCbntBuilderBase
{
    /// <summary>
    /// 测点组
    /// </summary>
    public ITagCbnt TagCbnt { get; }

    /// <summary>
    /// 组合名
    /// </summary>
    public virtual string Name => TagCbnt.Name;
    /// <summary>
    /// 起始地址
    /// </summary>
    public virtual string StartAddress => TagCbnt.StartAddress;

    private const string unknown_name = "(unknown_tag_name)";
    private const string unknown_address = "(unknown_start_address)";

    public TagCbntBuilderBase()
    {
        this.TagCbnt = new TagCbnt(unknown_name, unknown_address);
    }

    public virtual TagCbntBuilderBase WithXElement(XElement cbntElement)
    {
        var name = cbntElement.GetTagUnionName();
        var addr = cbntElement.GetTagUnionAddress(name);

        this.WithName(name);
        this.WithStartAddress(addr);
        return this;
    }


    public virtual TagCbntBuilderBase WithName(string name)
    {
        TagCbnt.Name = name;
        return this;
    }

    public virtual TagCbntBuilderBase WithStartAddress(string startAddress)
    {
        TagCbnt.StartAddress = startAddress;
        return this;
    }

    public virtual TagCbntBuilderBase WithChannel(ITagChannel channel)
    {
        TagCbnt.Channel = channel;
        return this;
    }

    public virtual TagCbntBuilderBase WithAccessMode(TagAccessMode accessMode)
    {
        TagCbnt.AcessMode = accessMode;
        return this;
    }

    public virtual TagCbntBuilderBase WithInterval(int interval)
    {
        TagCbnt.ScanInterval = interval;
        return this;
    }

    public virtual TagCbntBuilderBase WithIsEnabled(bool enabled)
    {
        TagCbnt.IsEnabled = enabled;
        return this;
    }

    public virtual TagCbntBuilderBase AddTag(ITagCbntor tag)
    {
        TagCbnt.Children.Add(tag.TagName(), tag);
        return this.WithAccessMode(tag.AccessMode());
    }

    public virtual TagCbntBuilderBase Configure(Action<TagCbntBuilderBase> action)
    {
        action?.Invoke(this);
        return this;
    }

    public abstract TagCbntBuilderBase AddTags(IList<TagDescriptor> descriptors);

    public virtual TagCbntBuilderBase AutoResize()
    {
        var cacheSize = 0;
        foreach (var kvp in this.TagCbnt.Children)
        {
            var tag = kvp.Value;
            var ending = tag.TagOffset + tag.TagDescriptor.TagSize;
            if (ending > cacheSize)
            {
                cacheSize = ending;
            }
        }
        this.TagCbnt.ResizeCache(cacheSize);
        return this;
    }

    public virtual ITagCbnt Build()
    {
        this.AutoResize();
        return this.TagCbnt;
    }
}
