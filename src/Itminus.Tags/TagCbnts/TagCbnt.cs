using System.Runtime.CompilerServices;

namespace Itminus.Tags;

/// <summary>
/// 代表一组测点组合，且底层通道必须是 <see cref="IContinousBytesBasedTagChannel"/>。<br/>
/// 为了避免被误用到非<see cref="IContinousBytesBasedTagChannel"/>场景，这个类被定义成一个内部类。
/// 目前仅仅开放给 S7、ModbusTcp等由我自己内置实现的模块(意味着不必担心滥用问题)。
/// <br/>
/// 外部开发者绝不应该依赖这个类，而是要提供自己的实现——听起来有点麻烦，不过不必担心：因为除了读写通道，其他属性的定义和方法都极其简单而且直观。<br/>
/// </summary>
internal class TagCbnt : ITagCbnt
{
    internal TagCbnt(string name, string startAddress)
    {
        this.Name = name;
        this.StartAddress = startAddress;
    }

    /// <inheritdoc/>
    public string Name { get; set; } = "";

    /// <inheritdoc/>
    public ITagGrp? Parent { get; set; }

    /// <inheritdoc/>
    public ITagChannel? Channel { get; set; }

    /// <inheritdoc/>
    public string StartAddress { get; set; } = "";

    /// <inheritdoc/>
    public int CacheSize { get; set; }

    /// <inheritdoc/>
    public Memory<byte> Cache { get; private set; } = Memory<byte>.Empty;

    /// <summary>
    /// 调整缓存大小
    /// </summary>
    /// <param name="cacheSize"></param>
    public void ResizeCache(int cacheSize)
    {
        this.CacheSize = cacheSize;
        var cache = new byte[cacheSize];
        
        var len = Math.Min(cacheSize, this.Cache.Length);
        if(len > 0)
        {
            this.Cache.Span.Slice(0, len).CopyTo(cache);
        }
        this.Cache = cache;
    }

    /// <inheritdoc/>
    public virtual bool IsDirty { get; set; }

    #region 子节点
    /// <inheritdoc/>
    public IDictionary<string, ITagCbntor> Children { get; } = new Dictionary<string, ITagCbntor>();

    /// <inheritdoc/>
    public ITagCbntor this[string tagName] => this.Children.TryGetValue(tagName, out var tag) ? 
        tag : 
        throw new Exception($"TagCbnt({this.Name}) has no child who's name={tagName}");
    #endregion

    /// <inheritdoc/>
    public int ScanInterval { get; set; } = 200;

    /// <inheritdoc/>
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc/>
    public TagAccessMode AcessMode { get; set; } = TagAccessMode.RW;

    /// <inheritdoc/>
    public bool IsScaned { get; set; }

    /// <inheritdoc/>
    public virtual async Task ReadAsync(CancellationToken ct)
    {
        var channel0 = this.GetRequiredChannel();
        var channel = channel0 as IContinousBytesBasedTagChannel;
        if(channel is null)
        {
            throw new NotImplementedException($"通道组合({nameof(TagCbnt)})依赖于通道{nameof(IContinousBytesBasedTagChannel)}，但当前实际通道是{channel0.GetType().Name}，如有必要，请考虑提供自己的通道组合实现。当前测点组合名称={this.Name}");
        }
        var bytes = await channel.ReadAsync(this.StartAddress, this.CacheSize, ct);
        this.Cache = bytes.AsMemory();
        foreach(var kv in this.Children)
        {
            var tag = kv.Value;
            tag.NotifyTagRead();
        }
    }

    /// <inheritdoc/>
    public virtual async Task WriteAsync(CancellationToken ct)
    {
        var channel0 = this.GetRequiredChannel();
        var channel = channel0 as IContinousBytesBasedTagChannel;
        if (channel is null)
        {
            throw new NotImplementedException($"通道组合({nameof(TagCbnt)})依赖于通道{nameof(IContinousBytesBasedTagChannel)}，但当前实际通道是{channel0.GetType().Name}，如有必要，请考虑提供自己的通道组合实现。当前测点组合名称={this.Name}");
        }
        var bytes = this.Cache.ToArray();
        await channel.WriteAsync(this.StartAddress, bytes, ct);

        foreach (var kv in this.Children)
        {
            var tag = kv.Value;
            tag.NotifyTagWritten();
            tag.IsDirty = false;
        }
        this.IsDirty = false;
    }


}
