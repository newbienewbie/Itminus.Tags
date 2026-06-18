using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Threading.Channels;


namespace Itminus.Tags.ComScanner.Channels;

/// <summary>
/// 抽象基类，表示基于串口的Tag通道。<br/>
/// 每一次读取一个数据包（读取方法由子类定义）放入待处理的队列中，并触发DataReceived事件。
/// 外部可以通过<see cref="TryDequeueInput(out T?)"/> 方法从队列中读取数据包进行处理。
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ComChannelBase<T> :ITagChannel
{
    protected readonly ComChannelOption _opt;
    protected readonly ILogger<ComChannelBase<T>> _logger;

    public string? NewLine { get; }

    /// <summary>
    /// 消息通道的容量
    /// </summary>
    public int Capacity { get; }

    protected Channel<T> _channel;

    /// <inheritdoc/>
    public string ChannelName { get; }

    /// <summary>
    /// 驱动
    /// </summary>
    public abstract string Driver { get; }

    public SerialPort? SerialPort { get; private set; }
    protected readonly SemaphoreSlim _sema = new SemaphoreSlim(1);

    protected ComChannelBase(string channelName, ComChannelOption opt, ILogger<ComChannelBase<T>> logger)
    {
        this.ChannelName = channelName;
        this._opt = opt;
        this.Capacity = opt.ChannelCapacity <= 0 ? 1 : opt.ChannelCapacity;
        this._logger = logger;
        this.NewLine = opt.NewLine;

        this._channel = Channel.CreateBounded<T>(this.Capacity);
    }

    /// <summary>
    /// 确保连接。注意：如果已经连接，则不执行任何操作；如果未连接，则打开串口并启动轮询线程
    /// </summary>
    /// <param name="force"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
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

            // 清空缓存
            if(this._channel is not null)
            {
                this._channel.Writer.TryComplete();
            }
            this._channel = Channel.CreateBounded<T>(Capacity);
            // 启动轮询
            var t = new Thread(async () => await PollDataAsync(ct));
            t.Start();
        }
        finally
        {
            this._sema.Release();
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
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
        if (this.SerialPort is null)
        {
            return;
        }
        this._channel.Writer.TryComplete();
        this.SerialPort?.Dispose();
        this.SerialPort = null;
    }


    protected abstract Task<T> ParseDataAsync(SerialPort sport, CancellationToken ct);

    private async Task PollDataAsync(CancellationToken ct)
    {
        if (this.SerialPort is null)
        {
            throw new InvalidOperationException($"通道({this.ChannelName})的串口为null, 无法Poll");
        }
        var stream = this.SerialPort.BaseStream;
        using var reader = new StreamReader(stream);
        try
        {
            while (!ct.IsCancellationRequested)
            {
                T? data;
                var read = false;
                await this._sema.WaitAsync(ct);
                try
                {
                    read = true;
                    data = await ParseDataAsync(this.SerialPort,ct);
                }
                finally
                {
                    this._sema.Release();
                }

                if(read)
                {
                    await _channel.Writer.WriteAsync(data, ct);
                    DataReceived?.Invoke(this, data);
                }
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError("通道({channel})读取失败：{ex}", this.ChannelName, ex.Message);
            await this.DisconnectAsync(CancellationToken.None);
        }

    }
    
    /// <summary>
    /// 收到消息事件
    /// </summary>
    public event EventHandler<T>? DataReceived;

    /// <summary>
    /// 尝试从队列中读取输入，成功返回true，失败返回false
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public bool TryDequeueInput(out T? input)
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


    /// <summary>
    /// 写入串口数据。注意：如果串口未连接，则会抛出异常
    /// </summary>
    /// <param name="response"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task WriteAsync(string response)
    {
        if (this.SerialPort is null)
        {
            throw new InvalidOperationException($"通道({this.ChannelName})的串口为空");
        }
        await this._sema.WaitAsync();
        try
        {
            this.SerialPort.Write(response);
        }
        finally
        {
            this._sema.Release();
        }
    }

    /// <summary>
    /// 写入串口数据。注意：如果串口未连接，则会抛出异常
    /// </summary>
    /// <param name="response"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task WriteAsync(byte[] response,int offset, int count)
    {
        if (this.SerialPort is null)
        {
            throw new InvalidOperationException($"通道({this.ChannelName})的串口为空");
        }
        await this._sema.WaitAsync();
        try
        {
            this.SerialPort.Write(response, offset, count);
        }
        finally
        {
            this._sema.Release();
        }
    }

}
