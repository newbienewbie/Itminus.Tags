namespace Itminus.Tags.ModbusTcp;

public class ByteDirectTag : Tag<byte, ModbusTcpChannel>
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
        var addr = this.GetAddress();
        var values = addr.Area switch { 
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
            _ => throw new InvalidOperationException($"按字节读写，只支持 HoldingRegisters/InputRegisters，当前测点({this.TagName()}), 地址={addr.Area}")
        };
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

        if(addr.Area != RegisterKinds.HoldingRegisters)
        {
            throw new InvalidOperationException($"按字节写入，只支持 HoldingRegisters，当前测点({this.TagName()}), 地址={addr.Area}");
        }
        var values= await this._bubbleChannel.ModbusMaster!.ReadHoldingRegistersAsync(
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

        await this._bubbleChannel.ModbusMaster!.WriteMultipleRegistersAsync(
            addr.SlaveAddress,
            addr.StartPoint,
            new ushort[] { (ushort)newValue }
        );
        this.IsDirty = false;
        this.NotifyTagWritten(this._value);
    }

}
