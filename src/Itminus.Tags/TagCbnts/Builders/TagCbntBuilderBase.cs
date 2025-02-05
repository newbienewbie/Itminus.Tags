
using System.Threading.Channels;

namespace Itminus.Tags;


/// <summary>
/// 测点组合的构建器基类，用于构建一个测点组合。
/// </summary>
public abstract class TagCbntBuilderBase
{
    /// <summary>
    /// 测点组
    /// </summary>
    public ITagCbnt TagCbnt { get; }

    public TagCbntBuilderBase(string cbntName, string startAddress)
    {
        this.TagCbnt = new TagCbnt(cbntName, startAddress);
    }

    public virtual TagCbntBuilderBase WithName(string name)
    {
        TagCbnt.Name = name;
        return this;
    }

    public virtual TagCbntBuilderBase WithDevice(ITagChannel channel)
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
        this.TagCbnt.CacheSize = cacheSize;
        this.TagCbnt.Cache = new byte[cacheSize];
        return this;
    }

    public virtual ITagCbnt Build()
    {
        this.AutoResize();
        return this.TagCbnt;
    }
}
