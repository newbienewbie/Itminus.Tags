namespace Itminus.Tags.ModbusTcp;


/// <summary>
/// Modbus的DO点，地址范围00000~09999
/// </summary>
internal class OutputCoilDirectTag : Tag<bool, ModbusTcpChannel>
{

    /// <param name="descriptor"></param> 
    public OutputCoilDirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
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
        var flags = await this._bubbleChannel.ModbusMaster!.ReadCoilsAsync(addr.SlaveAddress, addr.StartPoint, 1);
        this._value = flags[0];
        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(this._value);
    }

    public override async Task WriteAsync(CancellationToken ct)
    {
        var addr = this.GetAddress();
        var flag = this._value;
        await this._bubbleChannel.ModbusMaster!.WriteSingleCoilAsync(addr.SlaveAddress, addr.StartPoint, flag);
        this.IsDirty = false;
        this.NotifyTagWritten(flag);
    }
}
