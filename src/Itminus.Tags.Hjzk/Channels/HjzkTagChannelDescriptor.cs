using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Itminus.Tags.Hjzk;

/// <summary>
/// Hjzk 通道描述符
/// </summary>
public class HjzkTagChannelDescriptor : TagChannelDescriptor
{
    /// <summary>
    /// IP 地址，默认 localhost
    /// </summary>
    public string IpAddr { get; set; } = "localhost";

    /// <summary>
    /// 端口号，默认 502
    /// </summary>
    public int Port { get; set; } = 502;


    /// <inheritdoc/>
    public override XElement ToXElement()
    {
        var ele = base.ToXElement();

        ele.SetOrAddChild(nameof(IpAddr), this.IpAddr);
        ele.SetOrAddChild(nameof(Port), this.Port);
        return ele;
    }
}

/// <summary>
/// conversions between <see cref="TagChannelDescriptor"/> and <see cref="HjzkTagChannelDescriptor"/>
/// </summary>
public static class TagChannelDescriptor_S7Extensions
{
    /// <summary>
    /// 转成 <see cref="HjzkTagChannelDescriptor"/>
    /// </summary>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static HjzkTagChannelDescriptor ToHjzkTagChannelDescriptor(this TagChannelDescriptor descriptor)
    {
        if (descriptor.Driver != HjzkNames.DriverName)
        {
            throw new InvalidOperationException($"通道驱动错误：期望 {HjzkNames.DriverName}，而当前为{descriptor.Driver}");
        }
        if (descriptor is HjzkTagChannelDescriptor d)
        {
            return d;
        }
        var res = new HjzkTagChannelDescriptor
        {
            Name = descriptor.Name,
            Driver = descriptor.Driver,
            Extras = descriptor.Extras,
            IpAddr = !descriptor.Extras.TryGetValue(nameof(HjzkTagChannelDescriptor.IpAddr), out var ipAddr) ?
                "localhost" :
                ipAddr.Value,
            Port = !descriptor.Extras.TryGetValue(nameof(HjzkTagChannelDescriptor.Port), out var portEle) ?
                502 :
                int.TryParse(portEle.Value, out var port) ?
                    port :
                    throw new ArgumentException($"配置的端口号不是整数"),
        };
        return res;
    }

}