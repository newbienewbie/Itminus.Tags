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
    public TagDescriptor TagDescriptor { get; private set; }
    public string Name => TagDescriptor.TagName;
    public ITagChannel Channel { get; set; }

    public TagBuilderBase(TagDescriptor tagDescriptor, ITagChannel tagChannel)
    {
        this.TagDescriptor = tagDescriptor;
        this.Channel = tagChannel;
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
