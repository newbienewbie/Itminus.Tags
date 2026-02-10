using Itminus.Tags.Hjzk;
using Itminus.Tags.ModbusTcp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.Hjzk;

public class HjzkChannel : ModbusTcpChannel
{
    public HjzkChannel(string channelName, ModbusTcpItem modbusItem, ILogger<HjzkChannel> logger) 
        : base(channelName, modbusItem, logger)
    {
    }

    public override string Driver => HjzkNames.DriverName;
}
