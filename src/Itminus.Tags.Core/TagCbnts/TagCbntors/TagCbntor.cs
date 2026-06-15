namespace Itminus.Tags;


/// <summary>
/// 默认的测点组合子。<br/>
/// <br/>
/// 注意:
/// 如果相应的通道不是 <see cref="IContinousBytesBasedTagChannel"/>，
/// 子类必须重写<see cref="WriteAsync(CancellationToken)"/>和<see cref="ReadAsync(CancellationToken)"/>两个方法。
/// </summary>
public abstract class TagCbntor : ITagCbntor
{
    public TagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int tagOffset, int cacheOffset)
    {
        TagDescriptor = tagDescriptor;
        TagCbnt = tagCbnt;
        TagOffset = tagOffset;
        CacheOffset = cacheOffset;
    }

    /// <summary>
    /// 测点描述
    /// </summary>
    public TagDescriptor TagDescriptor { get; set; } = null!;


    /// <summary>
    /// 所属的测点组合
    /// </summary>
    public ITagCbnt TagCbnt { get; set; } = null!;

    /// <summary>
    /// 通道
    /// </summary>
    public ITagChannel? Channel => TagCbnt.GetChannel();

    /// <summary>
    /// 父容器，指向所属的测点组合。<br/>
    /// </summary>
    public TagContainer? Parent { get; set; }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public int CacheOffset{ get; set; }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public virtual int TagOffset { get; set; }

    /// <inheritdoc />
    public abstract object? Value { get; set; }

    /// <inheritdoc />
    public virtual DateTime Timestamp { get; set; }

    /// <inheritdoc />
    public event TagSyncEventHandler? OnTagRead;

    /// <inheritdoc />
    public event TagSyncEventHandler? OnTagWritten;

    /// <summary>
    /// 通知值已经更新，这个方法不在乎值是否一样
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    public virtual void NotifyTagRead()
    {
        if (this.OnTagRead != null)
        {
            var eArgs = new TagSyncEventArgs(this.Value, this.Timestamp, TagSyncEventArgs.Kinds.Read);
            this.OnTagRead(this, eArgs);
        }
    }

    /// <inheritdoc/>
    public void NotifyTagWritten()
    {
        if (this.OnTagWritten != null)
        {
            var eArgs = new TagSyncEventArgs(this.Value, this.Timestamp, TagSyncEventArgs.Kinds.Written);
            this.OnTagWritten(this, eArgs);
        }
    }

    /// <inheritdoc/>
    public bool IsScaned { get; set; }

    #region 
    /// <inheritdoc />
    public virtual bool IsDirty { get; set; }


    protected virtual void MarkDirty()
    {
        this.IsDirty = true;
        this.TagCbnt.IsDirty = true;
    }
    #endregion

    /// <summary>
    /// 把当前测点值刷到底层。<br/>
    /// </summary>
    /// <remarks>
    /// 注意：基类提供了基于<see cref="IContinousBytesBasedTagChannel"/>的实现。如果不是该种通道，子类应该重写本方法，否则会抛出异常。
    /// </remarks>
    /// <returns></returns>
    public virtual async Task WriteAsync(CancellationToken ct)
    {
        var channel0 = this.TagCbnt.GetRequiredChannel();
        var channel = channel0 as IContinousBytesBasedTagChannel;
        if (channel is null)
        {
            throw new NotImplementedException($"通道组合子默认实现依赖于通道{nameof(IContinousBytesBasedTagChannel)}，但当前实际通道是{channel0.GetType().Name}。当前测点组合子名称={this.TagName()}");
        }

        var cache = this.TagCbnt.Cache.Slice(this.CacheOffset, this.TagSize());
        await channel.WriteAsync(this.NormalizedAddress(), cache.ToArray(),ct);
        this.NotifyTagWritten();
        this.IsDirty = false;
    }

    /// <summary>
    /// 从底层读取数据到当前测点值。<br/>
    /// </summary>
    /// <remarks>
    /// 注意：基类提供了基于<see cref="IContinousBytesBasedTagChannel"/>的实现。如果不是该种通道，子类应该重写本方法，否则会抛出异常。
    /// </remarks>
    /// <returns></returns>
    public virtual async Task ReadAsync(CancellationToken ct)
    {
        var channel0 = this.TagCbnt.GetRequiredChannel();
        var channel = channel0 as IContinousBytesBasedTagChannel;
        if (channel is null)
        {
            throw new NotImplementedException($"通道组合子默认实现依赖于通道{nameof(IContinousBytesBasedTagChannel)}，但当前实际通道是{channel0.GetType().Name}。当前测点组合子名称={this.TagName()}");
        }

        var bytes = await channel.ReadAsync(this.NormalizedAddress(), this.TagSize(),ct);
        var cache = this.TagCbnt.Cache.Slice(this.CacheOffset, bytes.Length);
        bytes.CopyTo(cache);
        this.NotifyTagRead();
    }

}
