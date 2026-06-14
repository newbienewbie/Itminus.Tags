namespace Itminus.Tags.ModbusTcp;

public class HoldingRegisterUInt16DirectTag : Tag<ushort>
{
    public HoldingRegisterUInt16DirectTag(TagDescriptor descriptor, TagContainer container)
        : base(descriptor, container)
    {
        this._mbChannel = this.GetModbusTcpChannel();
    }
    public override ITagChannel? Channel { get; set; }

    private readonly ModbusTcpChannel _mbChannel;

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
        var addr = this.GetAddress();
        var values= await this._mbChannel.ModbusMaster!.ReadHoldingRegistersAsync(
            addr.SlaveAddress, 
            addr.StartPoint, 
            1
        );
        var value = values[0];

        if(this.TagDescriptor.EndianKind == EndianKinds.BigEndian)
        {
            var loByte = value & 0x00FF;
            var hiByte = value >> 8;
            value = (ushort)(loByte << 8 | hiByte);
        }
        this._value = value;

        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(this._value);
    }
    public override async Task WriteAsync(CancellationToken ct) 
    {
        var addr = this.GetAddress();
        var value = this._value;
        if(this.TagDescriptor.EndianKind == EndianKinds.BigEndian)
        {
            var loByte = value & 0x00FF;
            var hiByte = value >> 8;
            value = (ushort)(loByte << 8 | hiByte);
        }

        await this._mbChannel.ModbusMaster!.WriteMultipleRegistersAsync(
            addr.SlaveAddress,
            addr.StartPoint,
            new ushort[] { value }
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
