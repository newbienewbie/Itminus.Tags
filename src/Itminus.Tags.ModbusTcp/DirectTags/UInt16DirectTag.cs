namespace Itminus.Tags.ModbusTcp;

public class UInt16DirectTag : Tag<ushort, ModbusTcpChannel>
{
    public UInt16DirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
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
        var addr = this.GetAddress();
        var values = addr.Area switch
        {
            RegisterKinds.HoldingRegisters => await this._bubbleChannel.ModbusMaster!.ReadHoldingRegistersAsync(
                addr.SlaveAddress, 
                addr.StartPoint, 
                1
            ),
            RegisterKinds.InputRegisters => await this._bubbleChannel.ModbusMaster!.ReadInputRegistersAsync(
                addr.SlaveAddress,
                addr.StartPoint,
                1
            ),
            _ => throw new InvalidOperationException($"按ushort读写，只支持 HoldingRegisters/InputRegisters，当前测点({this.TagName()}), 地址={addr.Area}")
        };
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
        if (addr.Area != RegisterKinds.HoldingRegisters)
        {
            throw new InvalidOperationException($"按字节写入，只支持 HoldingRegisters，当前测点({this.TagName()}), 地址={addr.Area}");
        }

        var value = this._value;
        if(this.TagDescriptor.EndianKind == EndianKinds.BigEndian)
        {
            var loByte = value & 0x00FF;
            var hiByte = value >> 8;
            value = (ushort)(loByte << 8 | hiByte);
        }

        await this._bubbleChannel.ModbusMaster!.WriteMultipleRegistersAsync(
            addr.SlaveAddress,
            addr.StartPoint,
            new ushort[] { value }
        );
        this.IsDirty = false;
        this.NotifyTagWritten(this._value);
    }

}
