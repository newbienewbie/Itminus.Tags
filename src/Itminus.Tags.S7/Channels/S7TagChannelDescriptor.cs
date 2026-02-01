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
        if (descriptor.Driver != S7Names.DriverName)
        {
            throw new InvalidOperationException($"通道驱动错误：期望 {S7Names.DriverName}，而当前为{descriptor.Driver}");
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

            IpAddr = !descriptor.Extras.TryGetValue(nameof(IpAddr), out var ipAddr) ?
                "localhost" :
                ipAddr.Value,
            Rack = !descriptor.Extras.TryGetValue(nameof(Rack), out var rackEle) ?
                (short)0:
                short.TryParse(rackEle.Value, out var rack) ?
                    rack:
                    throw new ArgumentException($"配置的Rack不是整数({rackEle.Value})"),
            Slot = !descriptor.Extras.TryGetValue(nameof(Slot), out var slotEle) ?
                (short)1 :
                short.TryParse(slotEle.Value, out var slot) ?
                    slot :
                    throw new ArgumentException($"配置的Slot不是整数({slotEle.Value})"),
        };
        return res;
    }
}

