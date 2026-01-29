using Itminus.Tags.ModbusTcp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.ZLan;

public class ZLanTcpChannel : ModbusTcpChannel
{
    public ZLanTcpChannel(string channelName, ModbusTcpItem modbusItem, ILogger<ModbusTcpChannel> logger) 
        : base(channelName, modbusItem, logger)
    {
    }

    public override string Driver => ZLanTcpNames.DriverName;
}
