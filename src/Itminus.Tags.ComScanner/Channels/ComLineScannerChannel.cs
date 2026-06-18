using Microsoft.Extensions.Logging;
using System.IO.Ports;

namespace Itminus.Tags.ComScanner.Channels;

/// <summary>
/// 基于行的串口通道。每一次读取一行
/// </summary>
public class ComLineScannerChannel : ComChannel<string>
{


    public ComLineScannerChannel(string channelName, ComScannerOption opt, ILogger<ComLineScannerChannel> logger)
        :base(channelName, opt, logger)
    {
    }

    public override string Driver => ComScannerNames.DriverName;

    protected override Task<string> ParseDataAsync(SerialPort sport)
    {
        var str = sport.ReadLine();
        return Task.FromResult(str);
    }
}
