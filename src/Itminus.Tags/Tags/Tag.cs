

namespace Itminus.Tags;

/// <summary>
/// 规定了测点值类型的抽象基类。<br/>
/// 注意：测点只需要实现<see cref="ITag"/>，并不一定要是这个基类的子类。比如<see cref="TagCbntor"/>就不是这个类的子类。<br/>
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Tag<T> : ITag
    where T : IEquatable<T>
{

    protected Tag(TagDescriptor descriptor)
    {
        TagDescriptor = descriptor;
    }

    public TagDescriptor TagDescriptor { get; set; }

    /// <summary>
    /// 读写通道
    /// </summary>
    public abstract ITagChannel Channel { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event TagSyncEventHandler? OnTagRead;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event TagSyncEventHandler? OnTagWritten;

    /// <inheritdoc/>
    public bool IsScaned { get; set; }

    #region 读写测点值
    private T _value = default!;


    object? ITag.Value
    {
        get => Value;
        set
        {
            Value = (T)value!;
        }
    }

    public virtual T Value
    {
        get => _value;
        set
        {
            _value = value;
            Timestamp = DateTime.UtcNow;
            IsDirty = true;
        }
    }

    public bool IsDirty { get; set; }


    public DateTime Timestamp { get; set; }
    #endregion


    /// <summary>
    /// 应该被子类调用
    /// </summary>
    /// <param name="newValue"></param>
    protected virtual void NotifyTagSynced(object? newValue)
    {
        if (this.OnTagRead != null)
        {
            var eArgs = new TagSyncEventArgs(newValue, this.Timestamp);
            this.OnTagRead(this, eArgs);
        }
    }

    /// <summary>
    /// 向底层写入，子类的实现必须调用 <see cref="NotifyTagSynced"/>, 并且 设置 <see cref="IsDirty"/> = false
    /// </summary>
    /// <returns></returns>
    public abstract Task WriteAsync(CancellationToken ct);

    /// <summary>
    /// 从底层读取，子类的实现必须调用 <see cref="NotifyTagSynced"/>
    /// </summary>
    /// <returns></returns>
    public abstract Task ReadAsync(CancellationToken ct);
}
