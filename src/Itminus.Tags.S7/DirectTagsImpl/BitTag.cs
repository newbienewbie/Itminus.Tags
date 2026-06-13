namespace Itminus.Tags.S7;


internal class BitTag : ContinousBytesBasedDirectTag<bool>
{
    /// <summary>
    /// 第Nth位比特: 取值范围 0~15。
    /// </summary>
    public byte NthBit { get; }

    /// <summary>
    /// c'tor
    /// </summary>
    /// <param name="descriptor"></param>
    /// <param name="thisChannel">自身通道</param>
    /// <param name="channel">冒泡式取得的通道</param>
    /// <param name="nthBit">比特位，通常取值范围[0,15]</param>
    /// <param name="bufferSize">缓存大小，如果比特位是[0,7],则可以取1；如果比特位是[0,15],则可以取2；默认自动计算</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public BitTag(TagDescriptor descriptor, ITagChannel? thisChannel, IContinousBytesBasedTagChannel channel, byte nthBit, int bufferSize=0) 
        : base(descriptor, thisChannel, channel)
    {
        this.NthBit = nthBit;
        this.BufferSize = bufferSize == 0 ? (this.NthBit / 8 + 1) : bufferSize;
    }

    /// <inheritdoc/>
    public override int BufferSize { get; }

    protected override bool ConvertFromBytes(Span<byte> bytes)
    {
        var index = this.NthBit / 8;
        var nth = this.NthBit % 8;
        var flags = bytes[index];
        var hasFlag = flags & (1 << nth);
        return hasFlag != 0;
    }

    protected override void FillBytes(Span<byte> bytes, bool value)
    {
        var index = this.NthBit / 8;
        var nth = this.NthBit % 8;
        var oldFlags = bytes[index];
        var newFlags = value ?
            oldFlags | 1 << nth :
            oldFlags & ~(1 << nth);
        bytes[index] = (byte)newFlags;
    }


    public override async Task WriteAsync(CancellationToken ct)
    {
        var addr = this.NormalizedAddress();
        var bytes = await this._channel.ReadAsync(addr, BufferSize, ct);
        this.FillBytes(bytes, this.Value);
        await this._channel.WriteAsync(addr, bytes, ct);
        this.NotifyTagWritten(this.Value);
        this.IsDirty = false;
    }
}



