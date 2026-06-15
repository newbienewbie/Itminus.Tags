

namespace Itminus.Tags;

/// <summary>
/// 规定了测点值类型的抽象基类。<br/>
/// 注意：测点只需要实现<see cref="ITag"/>，并不一定要是这个基类的子类。比如<see cref="TagCbntor"/>就不是这个类的子类。<br/>
/// </summary>
/// <typeparam name="TValue"></typeparam>
public abstract class Tag<TValue,TChannel> : ITag
    where TChannel: class, ITagChannel
{

    protected Tag(TagDescriptor descriptor, TChannel? thisChannel, TagContainer container)
    {
        this.TagDescriptor = descriptor;
        this.Channel = thisChannel;
        this.Parent = container;

        // init bubble channel
        var requiredChannel = this.GetRequiredChannel();
        if (requiredChannel is not TChannel ch)
        {
            var tagname = this.TagName();
            throw new InvalidOperationException($"测点(冒泡式)通道类型不对(测点={tagname},通道={requiredChannel.ChannelName} {typeof(TChannel).Name})");
        }
        this._bubbleChannel = ch;
    }

    /// <inheritdoc/>
    public TagDescriptor TagDescriptor { get; set; }

    /// <summary>
    /// 自身的读写通道
    /// </summary>
    public virtual ITagChannel? Channel { get; set; }

    /// <summary>
    /// 冒泡式通道
    /// </summary>
    protected readonly TChannel _bubbleChannel;

    /// <inheritdoc/>
    public TagContainer? Parent { get; set; }

    /// <inheritdoc/>
    public event TagSyncEventHandler? OnTagRead;

    /// <inheritdoc/>
    public event TagSyncEventHandler? OnTagWritten;

    /// <inheritdoc/>
    public bool IsScaned { get; set; }

    #region 读写测点值
    protected TValue? _value = default!;


    object? ITag.Value
    {
        get => Value;
        set
        {
            Value = (TValue?)value;
        }
    }

    public virtual TValue? Value
    {
        get => _value;
        set
        {
            _value = value;
            Timestamp = DateTime.Now;
            IsDirty = true;
        }
    }

    public bool IsDirty { get; set; }


    public DateTime Timestamp { get; set; }
    #endregion



    protected virtual void NotifyTagRead(object? newValue)
    {
        if (this.OnTagRead != null)
        {
            var eArgs = new TagSyncEventArgs(newValue, this.Timestamp, TagSyncEventArgs.Kinds.Read);
            this.OnTagRead(this, eArgs);
        }
    }

    protected virtual void NotifyTagWritten(object? newValue)
    {
        if (this.OnTagWritten != null)
        {
            var eArgs = new TagSyncEventArgs(newValue, this.Timestamp, TagSyncEventArgs.Kinds.Written);
            this.OnTagWritten(this, eArgs);
        }
    }

    /// <summary>
    /// 向底层写入，子类的实现必须调用 <see cref="NotifyTagWritten"/>, 并且 设置 <see cref="IsDirty"/> = false
    /// </summary>
    /// <returns></returns>
    public abstract Task WriteAsync(CancellationToken ct);

    /// <summary>
    /// 从底层读取并更新内部的 <see cref="_value"/>字段+ <see cref="Timestamp" />属性。<br/>
    /// 子类的实现必须调用 <see cref="NotifyTagRead"/>
    /// </summary>
    /// <returns></returns>
    public abstract Task ReadAsync(CancellationToken ct);
}
