namespace Itminus.Tags;


/// <summary>
/// 测点组合的构建器基类，用于构建一个测点组合。<br/>
/// 子类必须提供一个无参构造函数
/// </summary>
public abstract class TagCbntBuilderBase
{
    /// <summary>
    /// c'tor <br/>
    /// </summary>
    public TagCbntBuilderBase(ITagCbnt cbnt)
    {
        this.TagCbnt = cbnt;
    }

    /// <summary>
    /// 测点组
    /// </summary>
    public virtual ITagCbnt TagCbnt { get; protected set; }

    /// <summary>
    /// 父级测点组
    /// </summary>
    public virtual ITagGrp? Parent => TagCbnt.Parent;

    /// <summary>
    /// 组合名
    /// </summary>
    public virtual string Name => TagCbnt.Name;

    /// <summary>
    /// 起始地址
    /// </summary>
    public virtual string StartAddress => TagCbnt.StartAddress;

    /// <summary>
    /// 设置测点组合描述符<br/>
    /// 会被自动调用
    /// </summary>
    /// <param name="cbntDescriptor"></param>
    /// <returns></returns>
    public virtual TagCbntBuilderBase WithCbntDescriptor(TagCbntDescriptor cbntDescriptor)
    {
        this.WithName(cbntDescriptor.Name);
        this.WithStartAddress(cbntDescriptor.StartAddress);
        return this;
    }

    /// <summary>
    /// 设置测点组合名，返回自身。<br/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual TagCbntBuilderBase WithName(string name)
    {
        TagCbnt.Name = name;
        return this;
    }

    /// <summary>
    /// 设置测点组合起始地址，返回自身。<br/>
    /// </summary>
    /// <param name="startAddress"></param>
    /// <returns></returns>
    public virtual TagCbntBuilderBase WithStartAddress(string startAddress)
    {
        TagCbnt.StartAddress = startAddress;
        return this;
    }

    /// <summary>
    /// 设置通道，返回自身。<br/>
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public virtual TagCbntBuilderBase WithChannel(ITagChannel? channel)
    {
        TagCbnt.Channel = channel;
        return this;
    }

    /// <summary>
    /// 设置父级测点组<br/>
    /// 会被自动调用。
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    public virtual TagCbntBuilderBase WithParent(ITagGrp? parent)
    {
        this.TagCbnt.Parent = parent;
        return this;
    }

    /// <summary>
    /// 设置访问模式，返回自身。<br/>
    /// </summary>
    /// <param name="accessMode"></param>
    /// <returns></returns>
    public virtual TagCbntBuilderBase WithAccessMode(TagAccessMode accessMode)
    {
        TagCbnt.AcessMode = accessMode;
        return this;
    }

    /// <summary>
    /// 设置扫描间隔，返回自身。<br/>
    /// </summary>
    /// <param name="interval"></param>
    /// <returns></returns>
    public virtual TagCbntBuilderBase WithInterval(int interval)
    {
        TagCbnt.ScanInterval = interval;
        return this;
    }

    /// <summary>
    /// 设置是否启用，返回自身。<br/>
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns></returns>
    public virtual TagCbntBuilderBase WithIsEnabled(bool enabled)
    {
        TagCbnt.IsEnabled = enabled;
        return this;
    }

    /// <summary>
    /// 增加测点
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public virtual TagCbntBuilderBase AddTag(ITagCbntor tag)
    {
        tag.Parent = TagContainer.From(this.TagCbnt);
        TagCbnt.Children.Add(tag.TagName(), tag);
        return this.WithAccessMode(tag.AccessMode());
    }

    /// <summary>
    /// 配置测点组合构建器，返回自身。<br/>
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public virtual TagCbntBuilderBase Configure(Action<TagCbntBuilderBase> action)
    {
        action?.Invoke(this);
        return this;
    }

    /// <summary>
    /// 批量添加测点。<br/>
    /// 实现应该构造<see cref="ITagCbntor"/>，并调用<see cref="AddTag(ITagCbntor)"/>添加到测点组合中。<br/>
    /// </summary>
    /// <param name="descriptors"></param>
    /// <param name="channel">(冒泡式)获取的通道</param>
    /// <returns></returns>
    public abstract TagCbntBuilderBase AddTags(IList<TagDescriptor> descriptors, ITagChannel channel);


    /// <summary>
    /// 自动重算底层缓存区的大小需求，并对底层缓存区自动调整。会在<see cref="Build"/>中自动调用。<br/>
    /// 实现类需要根据自己实际情况，重写自己的布局算法。
    /// </summary>
    /// <returns></returns>
    protected abstract TagCbntBuilderBase AutoLayout();

    /// <summary>
    /// 构建测点组合，并返回构建好的测点组合。<br/>
    /// </summary>
    /// <param name="channel">(冒泡式)获取的通道</param>
    /// <returns></returns>
    public virtual ITagCbnt Build(ITagChannel channel)
    {
        this.AutoLayout();
        return this.TagCbnt;
    }
}
