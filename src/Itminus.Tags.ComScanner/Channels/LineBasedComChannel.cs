using Microsoft.Extensions.Logging;
using System.IO.Ports;

namespace Itminus.Tags.ComScanner.Channels;

/// <summary>
/// 基于行的串口通道。每一次读取一行
/// </summary>
public class LineBasedComChannel : ComChannelBase<string>
{


    public LineBasedComChannel(string channelName, ComChannelOption opt, ILogger<LineBasedComChannel> logger)
        :base(channelName, opt, logger)
    {
    }

    public override string Driver => ComScannerNames.DriverName;

    protected override Task<string> ParseDataAsync(SerialPort sport, CancellationToken ct)
    {
        var str = sport.ReadLine();
        return Task.FromResult(str);
    }
}
