

namespace Itminus.Tags;

public abstract class Tag<T> : ITag
    where T : IEquatable<T>
{

    protected Tag(TagDescriptor descriptor)
    {
        TagDescriptor = descriptor;
    }

    public TagDescriptor TagDescriptor { get; set; }

    public int CacheOffset { get; set; } = 0;

    /// <summary>
    /// 读写通道
    /// </summary>
    public abstract ITagChannel Channel { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event TagSyncEventHandler? OnTagSync;

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
        if (this.OnTagSync != null)
        {
            var eArgs = new TagSyncEventArgs(newValue, this.Timestamp);
            this.OnTagSync(this, eArgs);
        }
    }

    /// <summary>
    /// 向底层写入，子类的实现必须调用 <see cref="NotifyTagSynced"/>, 并且 设置 <see cref="IsDirty"/> = false
    /// </summary>
    /// <returns></returns>
    public abstract Task WriteAsync();

    /// <summary>
    /// 从底层读取，子类的实现必须调用 <see cref="NotifyTagSynced"/>
    /// </summary>
    /// <returns></returns>
    public abstract Task ReadAsync();
}
