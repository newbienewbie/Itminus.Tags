using Microsoft.Extensions.Logging;
using StdUnit.Sharp7.Options;
using StdUnit.Sharp7;
using Microsoft.FSharp.Core;

namespace Itminus.Tags.S7;


public class S7TagChannel : IContinousBytesBasedTagChannel
{

    public S7TagChannel(string channelName, S7PlcItem plc, ILogger<S7TagChannel> logger)
    {
        if (string.IsNullOrEmpty(channelName))
        {
            throw new ArgumentException($"'{nameof(channelName)}' cannot be null or empty", nameof(channelName));
        }

        this.ChannelName = channelName;
        PlcItem = plc;
        this._logger = logger;
    }

    private readonly ILogger<S7TagChannel> _logger;

    internal S7Client? Client { get; set; }
    public string ChannelName { get; set; } = DRIVER;
    public S7PlcItem PlcItem { get; }

    public string Driver => DRIVER;

    private static readonly string DRIVER = S7Names.DriverName;

    public virtual Task DisconnectAsync(CancellationToken ct)
    {
        if (this.Client == null)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource<Object?>();
        var th = new Thread(() => {
            try
            {
                this.Client?.Disconnect();
                this.Client = null;
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                this._logger.LogWarning("通道{ChannelName}断开连接失败：{message}\r\n{stackTrace}", ChannelName, ex.Message, ex.StackTrace);
                this.Client = null;
                tcs.SetException(ex);
            }
        });
        th.Start();
        return tcs.Task;
    }

    public virtual async Task EnsureConnectedAsync(bool force, CancellationToken ct)
    {
        //当前client存在并且连接有效
        if (!force && Client != null && Client.Connected)
        {
            return;
        }

        var result = await this.CreateClientAndConnectAsync(ct);
        if (result.IsError)
        {
            throw new Exception(result.ErrorValue.ToString());
        }
        this.Client = result.ResultValue;
    }

    protected virtual Task<FSharpResult<S7Client, ApiError>> CreateClientAndConnectAsync(CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<FSharpResult<S7Client, ApiError>>();
        var th = new Thread(() =>
        {
            var client = new S7Client();
            try
            {
                var code = client.ConnectTo(this.PlcItem.IpAddr, this.PlcItem.Rack, this.PlcItem.Slot);
                if (code == 0)
                {
                    tcs.SetResult(FSharpResult<S7Client, ApiError>.NewOk(client));
                }
                else
                {
                    tcs.SetResult(FSharpResult<S7Client, ApiError>.NewError(S7ErrorCodeHelper.GenerateApiError(this.ChannelName, code)));
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        th.Start();
        return tcs.Task;
    }

    /// <summary>
    /// 读取
    /// </summary>
    /// <param name="address"></param>
    /// <param name="length">要读取的字节数量</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public virtual Task<byte[]> ReadAsync(string address, int length, CancellationToken ct)
    {
        var addr = S7AddressParser.Parse(address);
        var buffer = new byte[length];
        if (addr.Area == AreaKinds.DB)
        {
            var code = this.Client!.DBRead(addr.BlockNumber, addr.StartAddress, length, buffer);
            if (code != 0)
            {
                var err = S7ErrorCodeHelper.GenerateApiError(this.ChannelName, code);
                throw new Exception(err.Text);
            }
        }
        else if (addr.Area == AreaKinds.MB) 
        { 
            var code = this.Client!.MBRead(addr.StartAddress, length, buffer);
            if(code != 0)
            {
                var err = S7ErrorCodeHelper.GenerateApiError(this.ChannelName, code);
                throw new Exception(err.Text);
            }
        }
        else
        {
            throw new NotImplementedException($"不支持的地址区域类型={addr.Area}");
        }

        return Task.FromResult(buffer);
    }

    /// <summary>
    /// 写入底层
    /// </summary>
    /// <param name="address"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public virtual Task WriteAsync(string address, byte[] buffer, CancellationToken ct)
    {
        var addr = S7AddressParser.Parse(address);
        if(addr.Area == AreaKinds.DB)
        {
            var code = this.Client!.DBWrite(addr.BlockNumber, addr.StartAddress, buffer.Length, buffer);
            if (code != 0)
            {
                var err = S7ErrorCodeHelper.GenerateApiError(this.ChannelName, code);
                throw new Exception(err.Text);
            }
        }
        else if(addr.Area == AreaKinds.MB)
        {
            var code = this.Client!.MBWrite(addr.StartAddress, buffer.Length, buffer);
            if (code != 0)
            {
                var err = S7ErrorCodeHelper.GenerateApiError(this.ChannelName, code);
                throw new Exception(err.Text);
            }
        }
        else
        {
            throw new NotImplementedException($"不支持的地址区域类型={addr.Area}");
        }
        return Task.CompletedTask;
    }


    public virtual void Dispose()
    {
        if (Client != null && Client.Connected)
        {
            try
            {
                this._logger.LogInformation("通道={ChannelName} 正在断开连接...", ChannelName);
                Client.Disconnect();
                this._logger.LogInformation("通道={ChannelName}  断开连接完成!", ChannelName);
            }
            catch (Exception e)
            {
                this._logger.LogWarning("通道={ChannelName} 释放异常:{exception}", ChannelName, e.Message);
            }
            finally
            {

            }
        }
    }
}