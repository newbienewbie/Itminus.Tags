namespace Itminus.Tags.ModbusTcp;


/// <summary>
/// Modbus的DO点，地址范围00000~09999
/// </summary>
public class OutputCoilDirectTag : Tag<bool>
{

    /// <param name="descriptor"></param> 
    public OutputCoilDirectTag(TagDescriptor descriptor)
        : base(descriptor)
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
        var channel = this.GetModbusTcpChannel();
        var addr = this.GetAddress();
        var flags = await channel.ModbusMaster!.ReadCoilsAsync(addr.SlaveAddress, addr.StartPoint, 1);
        this._value = flags[0];
        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(this._value);
    }

    public override async Task WriteAsync(CancellationToken ct)
    {
        var channel = this.GetModbusTcpChannel();
        var addr = this.GetAddress();
        var flag = this._value;
        await channel.ModbusMaster!.WriteSingleCoilAsync(addr.SlaveAddress, addr.StartPoint, flag);
        this.IsDirty = false;
        this.NotifyTagWritten(flag);
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
