using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.ModbusTcp;


public class ModbusTcpTagChannelDescriptor : ChannelDescriptor
{
    public string IpAddr { get; set; } = "localhost";

    public int Port { get; set; } = 502;


    public static ModbusTcpTagChannelDescriptor FromDescriptor(ChannelDescriptor descriptor)
    {
        if (descriptor.Driver != ModbusTcpNames.DriverName)
        {
            throw new InvalidOperationException($"通道驱动错误：期望 {ModbusTcpNames.DriverName}，而当前为{descriptor.Driver}");
        }
        if(descriptor is ModbusTcpTagChannelDescriptor d)
        {
            return d;
        }
        var res = new ModbusTcpTagChannelDescriptor
        {
            Name = descriptor.Name,
            Driver = descriptor.Driver,
            Extras = descriptor.Extras,
            IpAddr = descriptor.Extras.TryGetValue(nameof(IpAddr), out var ipAddr) ?
                ipAddr.GetString() ?? throw new Exception("IpAddr未配置") :
                "localhost",
            Port = descriptor.Extras.TryGetValue(nameof(Port), out var port) ?
                port.GetInt32() :
                502,
        };
        return res;
    }
}

