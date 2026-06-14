namespace Itminus.Tags.S7;

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
    /// <param name="parent">父容器</param>
    public ContinousBytesBasedDirectTag(TagDescriptor descriptor, ITagChannel? thisChannel, TagContainer parent) 
        : base(descriptor, parent)
    {
        this.Channel = thisChannel;
        this._ctChannel = this.GetRequreidContinousBytesBasedTagChannel();
    }

    /// <inheritdoc/>
    public override ITagChannel? Channel { get; set; }

    protected readonly IContinousBytesBasedTagChannel _ctChannel;

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
        var bytes = await this._ctChannel.ReadAsync(addr, BufferSize, ct);
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
        await this._ctChannel.WriteAsync(addr, bytes, ct);
        this.NotifyTagWritten(this.Value);
        this.IsDirty = false;
    }

    private IContinousBytesBasedTagChannel GetRequreidContinousBytesBasedTagChannel()
    {
        var channel = this.GetRequiredChannel() as IContinousBytesBasedTagChannel;
        if (channel is null)
        {
            throw new InvalidOperationException($"Tag({this.TagName()}) 的通道必须实现 {nameof(IContinousBytesBasedTagChannel)}");
        }

        return channel;
    }
}