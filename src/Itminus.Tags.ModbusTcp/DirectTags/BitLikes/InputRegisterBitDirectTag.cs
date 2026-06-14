namespace Itminus.Tags.ModbusTcp;

/// <summary>
/// Modbus的 InputRegister 的 bit 位，地址范围300000~399999
/// </summary>
public class InputRegisterBitDirectTag: Tag<bool, ModbusTcpChannel>
{
    public InputRegisterBitDirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
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
        var bytes= await this._mbChannel.ModbusMaster!.ReadInputRegistersAsync(addr.SlaveAddress, addr.StartPoint, count);
        var index = this.NthBit / 8;
        var nth = this.NthBit % 8;
        var flags = bytes[index];
        var flag = flags & (1 << nth);
        this._value = flag !=0;
        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(this._value);
    }
    public override async Task WriteAsync(CancellationToken ct) =>
        throw new NotSupportedException($"输入寄存器点不可写入({this.TagName}");

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
