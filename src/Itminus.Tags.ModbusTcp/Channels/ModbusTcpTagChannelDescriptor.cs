using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Itminus.Tags.ModbusTcp;

/// <summary>
/// ModbusTcp 通道描述符
/// </summary>
public class ModbusTcpTagChannelDescriptor : TagChannelDescriptor
{
    /// <summary>
    /// c'tor
    /// </summary>
    public ModbusTcpTagChannelDescriptor()
    {
        this.Driver = ModbusTcpNames.DriverName;
    }

    /// <summary>
    /// IP 地址，默认值为 localhost
    /// </summary>
    public string IpAddr { get; set; } = "localhost";

    /// <summary>
    /// 端口号，默认值为 502
    /// </summary>
    public int Port { get; set; } = 502;

    /// <summary>
    /// 单批次最多写入的寄存器数量。null 表示使用默认值。
    /// </summary>
    public ushort? MaxBatchSize { get; set; }

    /// <summary>
    /// 单帧最多读取的寄存器数量(FC03/FC04)。null 表示使用协议默认值(125)。<br/>
    /// 某些设备的单帧上限小于协议理论值，可通过此项配置更小的值。<br/>
    /// </summary>
    public ushort? MaxReadRegisters { get; set; }

    /// <summary>
    /// 单帧最多读取的位数/点数(FC01/FC02)。null 表示使用协议默认值(2000)。<br/>
    /// 某些设备的单帧上限小于协议理论值，可通过此项配置更小的值。<br/>
    /// </summary>
    public ushort? MaxReadBits { get; set; }

    /// <inheritdoc/>
    public override XElement ToXElement()
    {
        var ele = base.ToXElement();

        ele.SetOrAddChild(nameof(IpAddr), this.IpAddr);
        ele.SetOrAddChild(nameof(Port), this.Port);
        if (MaxBatchSize.HasValue)
        {
            ele.SetOrAddChild(nameof(MaxBatchSize), this.MaxBatchSize.Value.ToString());
        }
        if (MaxReadRegisters.HasValue)
        {
            ele.SetOrAddChild(nameof(MaxReadRegisters), this.MaxReadRegisters.Value.ToString());
        }
        if (MaxReadBits.HasValue)
        {
            ele.SetOrAddChild(nameof(MaxReadBits), this.MaxReadBits.Value.ToString());
        }
        return ele;
    }
}

/// <summary>
/// conversions between <see cref="TagChannelDescriptor"/> and <see cref="ModbusTcpTagChannelDescriptor"/>
/// </summary>
public static class TagChannelDescriptor_ModbusTcpExtensions
{
    /// <summary>
    /// 转成 <see cref="ModbusTcpTagChannelDescriptor"/>，如果当前对象已经是 <see cref="ModbusTcpTagChannelDescriptor"/>，则直接返回
    /// </summary>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static ModbusTcpTagChannelDescriptor ToModbusTcpTagChannelDescriptor(this TagChannelDescriptor descriptor)
    {
        if (descriptor.Driver != ModbusTcpNames.DriverName)
        {
            throw new InvalidOperationException($"通道驱动错误：期望 {ModbusTcpNames.DriverName}，而当前为{descriptor.Driver}");
        }
        if (descriptor is ModbusTcpTagChannelDescriptor d)
        {
            return d;
        }
        var res = new ModbusTcpTagChannelDescriptor
        {
            Name = descriptor.Name,
            Driver = descriptor.Driver,
            Extras = descriptor.Extras,
            IpAddr = !descriptor.Extras.TryGetValue(nameof(ModbusTcpTagChannelDescriptor.IpAddr), out var ipAddr) ?
                "localhost" :
                ipAddr.Value,
            Port = !descriptor.Extras.TryGetValue(nameof(ModbusTcpTagChannelDescriptor.Port), out var portEle) ?
                502 :
                int.TryParse(portEle.Value, out var port) ?
                    port :
                    throw new ArgumentException($"配置的端口号不是整数"),
            MaxBatchSize = !descriptor.Extras.TryGetValue(nameof(ModbusTcpTagChannelDescriptor.MaxBatchSize), out var batchEle) ?
                null :
                ushort.TryParse(batchEle.Value, out var batch) ?
                    batch :
                    throw new ArgumentException($"MaxBatchSize 配置不是整数"),
            MaxReadRegisters = !descriptor.Extras.TryGetValue(nameof(ModbusTcpTagChannelDescriptor.MaxReadRegisters), out var readRegsEle) ?
                null :
                !ushort.TryParse(readRegsEle.Value, out var readRegs) ?
                    throw new ArgumentException($"MaxReadRegisters 配置不是整数") :
                    readRegs == 0 ?
                        throw new ArgumentException($"MaxReadRegisters 必须大于 0") :
                        readRegs > ModbusTcpChannel.MaxReadRegistersPerPdu ?
                            throw new ArgumentException($"MaxReadRegisters 配置({readRegs})超过协议上限({ModbusTcpChannel.MaxReadRegistersPerPdu})") :
                            readRegs,
            MaxReadBits = !descriptor.Extras.TryGetValue(nameof(ModbusTcpTagChannelDescriptor.MaxReadBits), out var readBitsEle) ?
                null :
                !ushort.TryParse(readBitsEle.Value, out var readBits) ?
                    throw new ArgumentException($"MaxReadBits 配置不是整数") :
                    readBits == 0 ?
                        throw new ArgumentException($"MaxReadBits 必须大于 0") :
                        readBits > ModbusTcpChannel.MaxReadBitsPerPdu ?
                            throw new ArgumentException($"MaxReadBits 配置({readBits})超过协议上限({ModbusTcpChannel.MaxReadBitsPerPdu})") :
                            readBits,
        };
        return res;
    }
}