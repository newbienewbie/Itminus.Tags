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
    public abstract TagDescriptor TagDescriptor { get; protected set; }

    public abstract ITagChannel Channel { get; protected set; }

    public string Name => TagDescriptor.TagName;

    public TagBuilderBase WithTagDescriptor(TagDescriptor descriptor)
    {
        this.TagDescriptor = descriptor;
        return this;
    }


    public virtual TagBuilderBase WithChannel(ITagChannel channel)
    {
        this.Channel = channel;
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
    /// <returns></returns>
    public abstract ITag Build();
}
