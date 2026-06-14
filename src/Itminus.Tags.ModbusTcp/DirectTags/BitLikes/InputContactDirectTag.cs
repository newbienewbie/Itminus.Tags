namespace Itminus.Tags.ModbusTcp;


/// <summary>
/// ModBus的 DI 点，地址范围10000~19999
/// </summary>
public class InputContactDirectTag : Tag<bool, ModbusTcpChannel>
{
    public InputContactDirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container) 
        : base(descriptor, thisChannel, container)
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
        var addr = this.GetAddress();
        var flags = await this._bubbleChannel.ModbusMaster!.ReadInputsAsync(addr.SlaveAddress, addr.StartPoint, 1);
        this._value = flags[0];
        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(this._value);
    }


    public override async Task WriteAsync(CancellationToken ct) =>
        throw new NotSupportedException($"DI点({this.TagName}地址={this.RawAddress()})不可写入");


}
