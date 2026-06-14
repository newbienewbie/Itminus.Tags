namespace Itminus.Tags.ModbusTcp;

public class HoldingRegisterBitDirectTag : Tag<bool>
{
    public HoldingRegisterBitDirectTag(TagDescriptor descriptor, TagContainer container)
        : base(descriptor, container)
    {
        var addr = this.GetAddress();
        this.NthBit = addr.NthBit;

        this._mbChannel = this.GetModbusTcpChannel();
    }
    public override ITagChannel? Channel { get; set; }

    private readonly ModbusTcpChannel _mbChannel;

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

    private ushort GetBufferSize() => (ushort)(this.NthBit / 8 + 1);

    public override async Task ReadAsync(CancellationToken ct)
    {
        var addr = this.GetAddress();
        var count = this.GetBufferSize();
        var bytes = await this._mbChannel.ModbusMaster!.ReadHoldingRegistersAsync(
            addr.SlaveAddress, 
            addr.StartPoint, 
            count
        );
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
        var addr = this.GetAddress();
        var count = this.GetBufferSize();

        var bytes= await this._mbChannel.ModbusMaster!.ReadHoldingRegistersAsync(
            addr.SlaveAddress,
            addr.StartPoint,
            count
        );

        var index = this.NthBit / 8;
        var nth = this.NthBit % 8;
        var oldFlags = bytes[index];
        var flag = this._value?
            oldFlags | 1 << nth :
            oldFlags & ~(1 << nth);
        bytes[index] = (ushort) flag;

        await this._mbChannel.ModbusMaster!.WriteMultipleRegistersAsync(
            addr.SlaveAddress,
            addr.StartPoint,
            bytes
        );
        this.IsDirty = false;
        this.NotifyTagWritten(this._value);
    }

    private ModbusTcpChannel GetModbusTcpChannel()
    {
        var channel = this.GetRequiredChannel() as ModbusTcpChannel;
        if (channel is null)
        {
            var tagname = this.TagName();
            throw new InvalidOperationException($"Tag {tagname} is not associated with a ModbusTcpChannel.");
        }

        return channel;
    }
}
