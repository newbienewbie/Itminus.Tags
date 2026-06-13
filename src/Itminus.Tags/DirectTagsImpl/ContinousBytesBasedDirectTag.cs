namespace Itminus.Tags.DirectTags;

/// <summary>
/// 表示一个基于连续字节的直接测点。<br/>
/// </summary>
/// <typeparam name="T"></typeparam>
internal abstract class ContinousBytesBasedDirectTag<T> : Tag<T>
    where T: notnull, IEquatable<T>
{
    /// <summary>
    /// c'tor
    /// </summary>
    /// <param name="descriptor"></param>
    /// <param name="thisChannel">对应于测点本身的通道</param>
    /// <param name="channel">冒泡式获取的通道</param>
    public ContinousBytesBasedDirectTag(TagDescriptor descriptor, ITagChannel? thisChannel, IContinousBytesBasedTagChannel channel) 
        : base(descriptor)
    {
        this.Channel = thisChannel;
        this._channel = channel;
    }

    protected readonly IContinousBytesBasedTagChannel _channel;

    /// <inheritdoc/>
    public override ITagChannel? Channel { get; set; }

    /// <summary>
    /// 对应一个测点需要读写的字节数。<br/>
    /// </summary>
    public abstract int BufferSize { get; }

    protected abstract T ConvertFromBytes(Span<byte> bytes);
    protected abstract void FillBytes(Span<byte> bytes, T value);

    /// <inheritdoc/>
    public override async Task ReadAsync(CancellationToken ct)
    {
        var addr = this.NormalizedAddress();
        var bytes = await this._channel.ReadAsync(addr, BufferSize, ct);
        this._value = this.ConvertFromBytes(bytes.AsSpan());
        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(this.Value);
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(CancellationToken ct)
    {
        var addr = this.NormalizedAddress();
        var bytes = new byte[BufferSize];
        var value = this.Value ?? throw new InvalidOperationException($"Tag({this.TagName()}) 在写入前 Value 不能为空");
        this.FillBytes(bytes, value);
        await this._channel.WriteAsync(addr, bytes, ct);
        this.NotifyTagWritten(this.Value);
        this.IsDirty = false;
    }
}