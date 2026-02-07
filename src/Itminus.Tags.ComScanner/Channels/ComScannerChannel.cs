using Microsoft.Extensions.Logging;
using System.IO.Ports;

namespace Itminus.Tags.ComScanner.Channels;

public class ComScannerChannel : ITagChannel
{
    private readonly ScannerOption _opt;
    private readonly ILogger<ComScannerChannel> _logger;

    public ComScannerChannel(string channelName, ScannerOption opt, ILogger<ComScannerChannel> logger)
    {
        this.ChannelName = channelName;
        this._opt = opt;
        this._logger = logger;
    }

    public string ChannelName { get; }

    public string Driver => ComScannerNames.DriverName;

    public SerialPort? SerialPort { get; private set; }

    public string NewLine { get; } = "\r";

    private readonly SemaphoreSlim _sema = new SemaphoreSlim(1);

    public async Task EnsureConnectedAsync(bool force, CancellationToken ct)
    {
        await this._sema.WaitAsync();
        try
        {
            if (this.SerialPort != null)
            {
                return;
            }

            // 打开串口
            this.SerialPort = new SerialPort(this._opt.Port, this._opt.BaundRate, this._opt.Parity, this._opt.DataBits, this._opt.StopBits);
            this.SerialPort.Open();
            this.SerialPort.NewLine = this.NewLine;
        }
        finally
        {
            this._sema.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken ct)
    {
        await this._sema.WaitAsync();
        try
        {
            this.SerialPort?.Close();
            this.SerialPort?.Dispose();
        }
        finally
        {
            this.SerialPort = null;
            this._sema.Release();
        }
    }

    public void Dispose()
    {
        if(this.SerialPort is null)
        {
            return;
        }

        this.SerialPort?.Dispose();
        this.SerialPort=null;
    }

    private string? ReadInputLine(SerialPort serial)
    {
        this._sema.Wait();
        try
        {
            var str = serial.ReadLine();
            return str;
        }
        finally
        {
            this._sema.Release();
        }
    }

    public string? ReadString()
    {
        if (this.SerialPort is null)
        {
            throw new InvalidOperationException($"通道({this.ChannelName})的串口为空");
        }
        var input = ReadInputLine(this.SerialPort);
        this._logger.LogInformation("通道({ChannelName})收到扫码枪输入：{input}", this.ChannelName, input);
        return input?.TrimEnd(['\n', ' ']);
    }

    public Task<byte[]> ReadAsync(string address, int count, CancellationToken ct)
    {
        throw new InvalidOperationException($"扫码枪不支持连续地址读取");
    }

    public Task WriteAsync(string address, byte[] bytes, CancellationToken ct)
    {
        throw new InvalidOperationException($"扫码枪不支持写入");
    }
}
