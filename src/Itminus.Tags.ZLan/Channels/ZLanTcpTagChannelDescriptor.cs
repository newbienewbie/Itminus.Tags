using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Itminus.Tags.ZLan;


public class ZLanTcpTagChannelDescriptor : TagChannelDescriptor
{
    public string IpAddr { get; set; } = "localhost";

    public int Port { get; set; } = 502;




    public override XElement ToXElement()
    {
        var ele = base.ToXElement();

        ele.SetOrAddChild(nameof(IpAddr), this.IpAddr);
        ele.SetOrAddChild(nameof(Port), this.Port);
        return ele;
    }
}


public static class TagChannelDescriptor_S7Extensions
{
    public static ZLanTcpTagChannelDescriptor ToZLanTcpTagChannelDescriptor(this TagChannelDescriptor descriptor)
    {
        if (descriptor.Driver != ZLanTcpNames.DriverName)
        {
            throw new InvalidOperationException($"通道驱动错误：期望 {ZLanTcpNames.DriverName}，而当前为{descriptor.Driver}");
        }
        if (descriptor is ZLanTcpTagChannelDescriptor d)
        {
            return d;
        }
        var res = new ZLanTcpTagChannelDescriptor
        {
            Name = descriptor.Name,
            Driver = descriptor.Driver,
            Extras = descriptor.Extras,
            IpAddr = !descriptor.Extras.TryGetValue(nameof(ZLanTcpTagChannelDescriptor.IpAddr), out var ipAddr) ?
                "localhost" :
                ipAddr.Value,
            Port = !descriptor.Extras.TryGetValue(nameof(ZLanTcpTagChannelDescriptor.Port), out var portEle) ?
                502 :
                int.TryParse(portEle.Value, out var port) ?
                    port :
                    throw new ArgumentException($"配置的端口号不是整数"),
        };
        return res;
    }

}