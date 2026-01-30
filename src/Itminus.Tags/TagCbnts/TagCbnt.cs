using System.Runtime.CompilerServices;

namespace Itminus.Tags;

/// <summary>
/// 代表一组测点
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
        var channel = this.GetRequiredChannel();
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
        var channel = this.GetRequiredChannel();
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
