namespace Itminus.Tags.ModbusTcp;


/// <summary>
/// ModBus的 DI 点，地址范围10000~19999
/// </summary>
public class InputContactDirectTag : Tag<bool>
{
    public InputContactDirectTag(TagDescriptor descriptor, TagContainer container) 
        : base(descriptor, container)
    {
    }

    public override ITagChannel? Channel { get; set; }


    #region 地址
    private ModbusTcpAddress? _addr;


    protected ModbusTcpAddress GetAddress(){
        if(_addr.HasValue)
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
        var flags = await channel.ModbusMaster!.ReadInputsAsync(addr.SlaveAddress, addr.StartPoint, 1);
        this._value = flags[0];
        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(this._value);
    }


    public override async Task WriteAsync(CancellationToken ct) =>
        throw new NotSupportedException($"DI点({this.TagName}地址={this.RawAddress()})不可写入");

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
