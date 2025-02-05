namespace Itminus.Tags;

/// <summary>
/// 逻辑小组件基类
/// </summary>
public abstract class LogicetBase : ILogicet
{
    public LogicetBase(IList<ITagChannel> channels, ITagGrp tags)
    {
        this.Channels = channels ?? throw new Exception("构造逻辑组件时通道集不可为空");
        this.Tags = tags ?? throw new Exception("构造逻辑组件时测点集不可为空");
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual IList<ITagChannel> Channels { get; }

    /// <inheritdoc/>
    public virtual ITagGrp Tags { get; }

    /// <inheritdoc/>
    public abstract IDisposable Attach();
}