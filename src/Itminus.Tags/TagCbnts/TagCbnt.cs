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

    /// <summary>
    /// 底层的字节数组大小
    /// </summary>
    public int CacheSize { get; set; }

    /// <summary>
    /// 底层硬件映射的字节数组
    /// </summary>
    public Memory<byte> Cache { get; set; } = Memory<byte>.Empty;

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
    public TagAccessMode AcessMode { get; set; }

    /// <inheritdoc/>
    public bool IsScaned { get; set; }

    /// <inheritdoc/>
    public virtual async Task ReadAsync()
    {
        var channel = this.GetRequiredChannel();
        var bytes = await channel.ReadAsync(this.StartAddress, this.CacheSize);
        this.Cache = bytes.AsMemory();
        foreach(var kv in this.Children)
        {
            var tag = kv.Value;
            tag.NotifyValueUpdated();
        }
    }

    /// <inheritdoc/>
    public virtual async Task WriteAsync()
    {
        var channel = this.GetRequiredChannel();
        var bytes = this.Cache.ToArray();
        await channel.WriteAsync(this.StartAddress, bytes);

        foreach (var kv in this.Children)
        {
            var tag = kv.Value;
            tag.NotifyValueUpdated();
            tag.IsDirty = false;
        }
        this.IsDirty = false;
    }
}
