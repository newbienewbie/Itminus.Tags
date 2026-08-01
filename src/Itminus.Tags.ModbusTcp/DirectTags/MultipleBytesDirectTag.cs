namespace Itminus.Tags.ModbusTcp;

internal abstract class MultipleBytesDirectTag<T> : Tag<T, ModbusTcpChannel>
    where T: unmanaged, IEquatable<T>
{
    public MultipleBytesDirectTag(TagDescriptor descriptor, ModbusTcpChannel? thisChannel, TagContainer container)
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

    protected abstract int BufferSize { get; }

    protected abstract T GetValueFromBytes(byte[] bytes);
    protected abstract void FillBytes(T value, in Span<byte> buffer);

    /// <summary>
    /// 字节序模型（务必先理解再改代码）：<br/>
    /// <br/>
    /// 【cache 形态的由来】通道层把 NModbus 返回的 ushort[] 展平为 byte[]（每寄存器低字节在前）。<br/>
    /// 关键：NModbus 把线序大端字节解析成 ushort 数值，该数值在小端 CPU 的内存里天然是"低字节在前"——<br/>
    /// 所以 cache 的形态 = "设备存储经过 NModbus 解析 + CPU 字节序后的固有结果"，不是我们刻意选择的。<br/>
    /// <br/>
    /// 【还原规则】测点层用"匹配 cache 形态"的读取方式还原物理值（负负得正）：<br/>
    /// - 16 位（单寄存器，无寄存器间顺序）：cache = "读回数值的小端内存形态"<br/>
    ///   · 设备 BigEndian（读回数值 = 物理值）=> cache = 物理值小端形态 => Read*LittleEndian 还原<br/>
    ///   · 设备 LittleEndian（读回数值 = 物理值反字节）=> cache = 反字节的小端形态 => Read*BigEndian 还原<br/>
    /// - 32/64 位（多寄存器，含寄存器间顺序）：<br/>
    ///   · 设备 LittleEndian（低寄存器在前）=> cache 恰好 = 设备布局 => Read*LittleEndian 直读<br/>
    ///   · 设备 BigEndian（高寄存器在前）=> cache 与设备全反 => 逐寄存器交换后 Read*BigEndian（本方法）<br/>
    /// <br/>
    /// 各子类的 GetValueFromBytes/FillBytes 分支是上述规则的直接落地，改动前对照此模型。
    /// </summary>
    protected static byte[] SwapEachRegister(byte[] bytes)
    {
        var tmp = (byte[])bytes.Clone();
        for (int i = 0; i + 1 < tmp.Length; i += 2)
        {
            (tmp[i], tmp[i + 1]) = (tmp[i + 1], tmp[i]);
        }
        return tmp;
    }

    public override async Task ReadAsync(CancellationToken ct)
    {
        var bytes = await this._bubbleChannel.ReadAsync(this.NormalizedAddress(), cbSize: this.BufferSize, ct);
        this._value = this.GetValueFromBytes(bytes);
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
        // 底层暂不支持Span，留待以后优化成 stackalloc
        var buffer = new byte[this.BufferSize];
        this.FillBytes(value, buffer);
        
        await this._bubbleChannel.WriteAsync(this.NormalizedAddress(), buffer, ct);
        this.IsDirty = false;
        this.NotifyTagWritten(this._value);
    }

}
