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
    public HjzkChannel(HjzkTagChannelDescriptor descriptor, ILogger<HjzkChannel> logger) 
        : base(descriptor, logger)
    {
    }
}
