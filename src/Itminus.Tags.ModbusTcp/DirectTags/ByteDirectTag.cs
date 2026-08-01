namespace Itminus.Tags.ModbusTcp;

internal class ByteDirectTag : Tag<byte, ModbusTcpChannel>
{
    public ByteDirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
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

    public override async Task ReadAsync(CancellationToken ct)
    {
        // 通道层返回的字节数组是每寄存器小端序：[低字节, 高字节]
        var bytes = await this._bubbleChannel.ReadAsync(this.NormalizedAddress(), 2, ct);
        byte lowByte = bytes[0];
        byte highByte = bytes[1];

        this._value = this.TagDescriptor.EndianKind is EndianKinds.BigEndian ? highByte : lowByte;
        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(this._value);
    }
    public override async Task WriteAsync(CancellationToken ct) 
    {
        // read-modify-write：读出当前 2 字节，替换目标字节后写回
        var bytes = await this._bubbleChannel.ReadAsync(this.NormalizedAddress(), 2, ct);
        byte lowByte = bytes[0];
        byte highByte = bytes[1];

        var newValue = this.TagDescriptor.EndianKind is EndianKinds.BigEndian ? 
            (this._value << 8 | lowByte) : 
            (highByte << 8 | this._value);

        // 通道层期望小端字节序：[低字节, 高字节]
        await this._bubbleChannel.WriteAsync(this.NormalizedAddress(), new byte[] { (byte)newValue, (byte)(newValue >> 8) }, ct);
        this.IsDirty = false;
        this.NotifyTagWritten(this._value);
    }

}
