using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags;

/// <summary>
/// TagBuilder基类，用于构建 <see cref="ITag"/> 实例
/// </summary>
public abstract class TagBuilderBase
{
    /// <summary>
    /// 测点描述。<br/>
    /// 会被自动设置，通常不需要手动调用
    /// </summary>
    public virtual TagDescriptor TagDescriptor { get; protected set; } = null!;

    /// <summary>
    /// 测点自身的通道。<br/>
    /// 会被自动设置，通常不需要手动调用
    /// </summary>
    public virtual ITagChannel? Channel { get; protected set; } = null;

    /// <summary>
    /// 测点所属的群组。
    /// 会被自动设置，通常不需要手动调用
    /// </summary>
    public virtual ITagGrp Parent { get; protected set; } = null!;

    /// <summary>
    /// Tag's Name
    /// </summary>
    public string Name => TagDescriptor.TagName;


    /// <summary>
    /// 会被自动调用以设置 <see cref="TagDescriptor"/> 属性，通常不需要手动调用
    /// </summary>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public TagBuilderBase WithTagDescriptor(TagDescriptor descriptor)
    {
        this.TagDescriptor = descriptor;
        return this;
    }

    /// <summary>
    /// 会被自动调用以设置 <see cref="ITagChannel"/> 属性，通常不需要手动调用
    /// </summary>
    /// <param name="channel">配置测点本身的通道</param>
    /// <returns></returns>
    public virtual TagBuilderBase WithChannel(ITagChannel? channel)
    {
        this.Channel = channel;
        return this;
    }

    /// <summary>
    /// 会被自动调用以设置 <see cref="Parent"/> 属性，通常不需要手动调用
    /// </summary>
    /// <param name="grp"></param>
    /// <returns></returns>
    public virtual TagBuilderBase WithParent(ITagGrp grp)
    {
        this.Parent = grp;
        return this;
    }

    public virtual TagBuilderBase Configure(Action<TagBuilderBase> action)
    {
        action?.Invoke(this);
        return this;
    }

    /// <summary>
    /// 构建 <see cref="ITag"/> 实例
    /// </summary>
    /// <param name="channel">(冒泡式得到的)通道</param>
    /// <returns></returns>
    public abstract ITag Build(ITagChannel channel);
}
