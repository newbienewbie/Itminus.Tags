namespace Itminus.Tags.ModbusTcp;

public class HoldingRegisterByteDirectTag : Tag<byte>
{
    public HoldingRegisterByteDirectTag(TagDescriptor descriptor, TagContainer container)
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
        byte highByte = (byte)(value >> 8);
        var lowByte = (byte)(value & 0x00FF);

        this._value = this.TagDescriptor.EndianKind is EndianKinds.BigEndian ? highByte : lowByte;
        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(this._value);
    }
    public override async Task WriteAsync(CancellationToken ct) 
    {
        var addr = this.GetAddress();

        var values= await this._mbChannel.ModbusMaster!.ReadHoldingRegistersAsync(
            addr.SlaveAddress,
            addr.StartPoint,
            1
        );
        var oldvalue = values[0];
        byte highByte = (byte)(oldvalue >> 8);
        var lowByte = (byte)(oldvalue & 0x00FF);

        var newValue = this.TagDescriptor.EndianKind is EndianKinds.BigEndian ? 
            (this._value << 8 | lowByte) : 
            (highByte << 8 | this._value);

        await this._mbChannel.ModbusMaster!.WriteMultipleRegistersAsync(
            addr.SlaveAddress,
            addr.StartPoint,
            new ushort[] { (ushort)newValue }
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
