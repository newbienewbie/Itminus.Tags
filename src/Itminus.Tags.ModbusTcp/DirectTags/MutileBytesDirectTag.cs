namespace Itminus.Tags.ModbusTcp;

public abstract class MutileBytesDirectTag<T> : Tag<T, ModbusTcpChannel>
    where T: unmanaged, IEquatable<T>
{
    public MutileBytesDirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }
    public override ITagChannel? Channel { get; set; }

    #region 地址
    private ModbusTcpAddress? _addr;

    protected ModbusTcpAddress GetAddress()
    {
        if (_addr.HasValue)
        {
            return _addr.Value;
        }

        var addressStr = this.NormalizedAddress();
        var addr = ModBusTcpAddressParser.Parse(addressStr);
        this._addr = addr;
        return addr;
    }
    #endregion

    protected abstract int BufferSize { get; }

    protected abstract T GetValueFromBytes(byte[] bytes);
    protected abstract void FillBytes(T value, in Span<byte> buffer);

    public override async Task ReadAsync(CancellationToken ct)
    {
        var bytes = await this._bubbleChannel.ReadAsync(this.NormalizedAddress(), count: this.BufferSize, ct);
        this._value = this.GetValueFromBytes(bytes);
        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(this._value);
    }

    public override async Task WriteAsync(CancellationToken ct) 
    {
        var addr = this.GetAddress();
        if (addr.Area != RegisterKinds.HoldingRegisters)
        {
            throw new InvalidOperationException($"按字节写入，只支持 HoldingRegisters，当前测点({this.TagName()}), 地址={addr.Area}");
        }

        var value = this._value;
        // 底层暂不支持Span，留待以后优化成 stackalloc
        var buffer = new byte[this.BufferSize];
        this.FillBytes(value, buffer);
        
        await this._bubbleChannel.WriteAsync(this.NormalizedAddress(), buffer, ct);
        this.IsDirty = false;
        this.NotifyTagWritten(this._value);
    }

}
