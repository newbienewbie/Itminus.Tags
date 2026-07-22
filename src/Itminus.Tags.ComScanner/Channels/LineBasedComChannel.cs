using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Net;

namespace Itminus.Tags.ComScanner.Channels;

/// <summary>
/// 基于行的串口通道。每一次读取一行
/// </summary>
public class LineBasedComChannel : ComChannelBase<string>
{

    /// <summary>
    /// c'tor
    /// </summary>
    public LineBasedComChannel(string channelName, ComChannelOption opt, ILogger<LineBasedComChannel> logger)
        :base(channelName, opt, logger)
    {
        this.ReadEntireLine = opt.ReadEntireLine;
    }

    /// <inheritdoc/>
    public override string Driver => ComDriverNames.DriverName;

    /// <summary>
    /// 读取整行？
    /// </summary>
    public bool ReadEntireLine {get; set;} = true;

    /// <inheritdoc/>
    protected override async Task<string> ParseDataAsync(ISerialPortHandle sport, CancellationToken ct)
    {
        string? str = null;
        do
        {
            str = this.ReadEntireLine ?
                sport.ReadLine() : 
                sport.ReadExisting();
            await Task.Yield();
        }
        while (string.IsNullOrEmpty(str) && !ct.IsCancellationRequested);
        return str;
    }
}
