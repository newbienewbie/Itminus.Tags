namespace Itminus.Tags.ModbusTcp;

internal class HoldingRegisterBitDirectTag : Tag<bool, ModbusTcpChannel>
{
    public HoldingRegisterBitDirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
        var addr = this.GetAddress();
        this.NthBit = addr.NthBit;
    }

    public override ITagChannel? Channel { get; set; }


    /// <summary>
    /// 第Nth位比特: 取值范围 0~15。
    /// </summary>
    public byte NthBit { get; }

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

    private ushort GetBufferSize() => 2;   // 寄存器是 16 位，bit0~15 都在同一个寄存器内，读取 1 个寄存器(2 字节)

    public override async Task ReadAsync(CancellationToken ct)
    {
        var bytes = await this._bubbleChannel.ReadAsync(this.NormalizedAddress(), this.GetBufferSize(), ct);
        var index = this.NthBit / 8;
        var nth = this.NthBit % 8;
        var flags = bytes[index];
        var flag = flags & (1 << nth);
        this._value = flag != 0;
        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(this._value);
    }
    public override async Task WriteAsync(CancellationToken ct) 
    {
        var count = this.GetBufferSize();

        var bytes = await this._bubbleChannel.ReadAsync(this.NormalizedAddress(), count, ct);

        var index = this.NthBit / 8;
        var nth = this.NthBit % 8;
        var oldFlags = bytes[index];
        var flag = this._value?
            oldFlags | 1 << nth :
            oldFlags & ~(1 << nth);
        bytes[index] = (byte) flag;

        await this._bubbleChannel.WriteAsync(this.NormalizedAddress(), bytes, ct);
        this.IsDirty = false;
        this.NotifyTagWritten(this._value);
    }
}
