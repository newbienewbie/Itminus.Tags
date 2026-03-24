using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Threading.Channels;


namespace Itminus.Tags.ComScanner.Channels;


/// <summary>
/// 基于行的串口通道。每一次读取一行
/// </summary>
public class ComScannerChannel : ITagChannel
{
    private readonly ComScannerOption _opt;
    private readonly ILogger<ComScannerChannel> _logger;


    public int Capacity { get; }

    private Channel<string> _channel;

    public ComScannerChannel(string channelName, ComScannerOption opt, ILogger<ComScannerChannel> logger)
    {
        this.ChannelName = channelName;
        this._opt = opt;
        this.NewLine = opt.NewLine;
        this.Capacity = opt.ChannelCapacity <=0 ? 1 : opt.ChannelCapacity;
        this._logger = logger;

        this._channel = Channel.CreateBounded<string>(this.Capacity);
    }

    public string ChannelName { get; }

    public string Driver => ComScannerNames.DriverName;

    public SerialPort? SerialPort { get; private set; }

    public string? NewLine { get; }

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
            if(!string.IsNullOrEmpty(this.NewLine))
            {
                this.SerialPort.NewLine = this.NewLine;
            }

            // 清空缓存
            this._channel = Channel.CreateBounded<string>(Capacity);
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
            this._channel.Writer.TryComplete();
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
        this._channel.Writer.TryComplete();
        this.SerialPort?.Dispose();
        this.SerialPort=null;
    }



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
                await _channel.Writer.WriteAsync(data, ct);
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
        if (!this._channel.Reader.TryRead(out input))
        {
            return false;
        }
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
