using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO.Ports;

namespace Itminus.Tags.ComScanner.Channels;

public class ComScannerChannel : ITagChannel
{
    private readonly ComScannerOption _opt;
    private readonly ILogger<ComScannerChannel> _logger;

    public ComScannerChannel(string channelName, ComScannerOption opt, ILogger<ComScannerChannel> logger)
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
        await this._sema.WaitAsync(ct);
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
            
            // 清空缓存
            this._buffer.Clear();
            // 启动轮询
            var t = new Thread(async() => await PollDataAsync(ct));
            t.Start();
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
        this._buffer.Clear();
        this.SerialPort?.Dispose();
        this.SerialPort=null;
    }

    private ConcurrentQueue<string> _buffer = new ConcurrentQueue<string>();

    private async Task PollDataAsync(CancellationToken ct)
    {
        if (this.SerialPort is null)
        {
            throw new InvalidOperationException($"通道({this.ChannelName})的串口为空");
        }
        var stream = this.SerialPort.BaseStream;
        using var reader = new StreamReader(stream);
        try
        {
            while (!ct.IsCancellationRequested)
            {
                string data = this.SerialPort.ReadLine();
                _buffer.Enqueue(data);
                DataReceived?.Invoke(this, data);
            }
        }
        catch(Exception ex) 
        {
            this._logger.LogError("通道({channel})读取失败：{ex}", this.ChannelName, ex.Message);
            await this.DisconnectAsync(CancellationToken.None);
        }

    }
    public event EventHandler<string>? DataReceived;

    public bool TryDequeueInput(out string? input)
    {
        if (this.SerialPort is null)
        {
            throw new InvalidOperationException($"通道({this.ChannelName})的串口为空");
        }
        if (!this._buffer.TryDequeue(out input))
        {
            return false;
        }
        input = input?.TrimEnd(['\n', ' ']);
        this._logger.LogInformation("通道({ChannelName})收到扫码枪输入：{input}", this.ChannelName, input);
        return true;
    }


    public void Write(string response)
    {
        if (this.SerialPort is null)
        {
            throw new InvalidOperationException($"通道({this.ChannelName})的串口为空");
        }

        this.SerialPort.Write(response);
    }
}
