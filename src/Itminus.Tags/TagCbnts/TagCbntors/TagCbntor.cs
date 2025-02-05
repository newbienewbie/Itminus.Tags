using System.ComponentModel;

namespace Itminus.Tags;


/// <summary>
/// 默认的测点组合子
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
    public event TagSyncEventHandler? OnTagSync;

    /// <summary>
    /// 通知值已经更新，这个方法不在乎值是否一样
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    public virtual void NotifyValueUpdated()
    {
        if (this.OnTagSync != null)
        {
            var eArgs = new TagSyncEventArgs(this.Value, this.Timestamp);
            this.OnTagSync(this, eArgs);
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

    /// <inheritdoc />
    public virtual async Task WriteAsync()
    {
        var channel = this.TagCbnt.GetRequiredChannel();
        var cache = this.TagCbnt.Cache.Slice(this.CacheOffset, this.TagSize());
        await channel.WriteAsync(this.TagAddress(), cache.ToArray());
        this.NotifyValueUpdated();
        this.IsDirty = false;
    }

    /// <inheritdoc />
    public virtual async Task ReadAsync()
    {
        var channel = this.TagCbnt.GetRequiredChannel();
        var bytes = await channel.ReadAsync(this.TagAddress(), this.TagSize());
        var cache = this.TagCbnt.Cache.Slice(this.CacheOffset, bytes.Length);
        bytes.CopyTo(cache);
        this.NotifyValueUpdated();
    }
}
