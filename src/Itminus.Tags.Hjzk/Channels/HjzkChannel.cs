using Itminus.Tags.ModbusTcp;
using Microsoft.Extensions.Logging;


namespace Itminus.Tags.Hjzk;

/// <summary>
/// HJZK 通道
/// </summary>
public class HjzkChannel : ModbusTcpChannel
{

    /// <summary>
    /// c'tor
    /// </summary>
    public HjzkChannel(string channelName, ModbusTcpItem modbusItem, ILogger<HjzkChannel> logger) 
        : base(channelName, modbusItem, logger)
    {
    }

    /// <inheritdoc/>
    public override string Driver => HjzkNames.DriverName;
}
