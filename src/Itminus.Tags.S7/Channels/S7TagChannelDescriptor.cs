using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.S7;


public class S7TagChannelDescriptor : TagChannelDescriptor
{
    public string IpAddr { get; set; } = "localhost";
    public short Rack { get; set; } = 0;
    public short Slot { get; set; } = 1;


    public static S7TagChannelDescriptor FromDescriptor(TagChannelDescriptor descriptor)
    {
        if (descriptor.Driver != "S7")
        {
            throw new InvalidOperationException($"通道驱动错误：期望S7，而当前为{descriptor.Driver}");
        }
        if(descriptor is S7TagChannelDescriptor d)
        {
            return d;
        }
        var res = new S7TagChannelDescriptor
        {
            Name = descriptor.Name,
            Driver = descriptor.Driver,
            Extras = descriptor.Extras,
            IpAddr = descriptor.Extras.TryGetValue(nameof(IpAddr), out var ipAddr) ?
                ipAddr.GetString() ?? throw new Exception("IpAddr未配置") :
                "localhost",
            Rack = descriptor.Extras.TryGetValue(nameof(Rack), out var rack) ?
                rack.GetInt16() :
                (short)0,
            Slot = descriptor.Extras.TryGetValue(nameof(Slot), out var slot) ?
                slot.GetInt16() :
                (short)0
        };
        return res;
    }
}

