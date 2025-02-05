using FutureTech.Protocols;
using Microsoft.Extensions.Logging;
using NModbus;
using System.Net.Sockets;

namespace Itminus.Tags.ModbusTcp;

public class ModbusTcpChannel : ITagChannel
{
    private readonly ILogger<ModbusTcpChannel> _logger;

    private SemaphoreSlim _connSignal = new SemaphoreSlim(1, 1);

    #region 配置
    private readonly ModbusTcpItem _modbusItem;
    /// <summary>
    /// IP 地址
    /// </summary>
    public string IpAddr => _modbusItem.IpAddr;

    /// <summary>
    /// 端口号
    /// </summary>
    public int Port => _modbusItem.Port;

    /// <summary>
    /// 连接超时
    /// </summary>
    public int ConnTimeout => _modbusItem.ConnTimeout;

    /// <summary>
    /// 读超时
    /// </summary>
    public int ReadTimeout => _modbusItem.ReadTimeout;

    /// <summary>
    /// 写超时
    /// </summary>
    public int WriteTimeout => _modbusItem.WriteTimeout;
    #endregion



    public string ChannelName { get; }
    public string Driver => "ModbusTcp";

    public ModbusTcpChannel(string channelName, ModbusTcpItem modbusItem, ILogger<ModbusTcpChannel> logger)
    {
        ChannelName = channelName;
        _modbusItem = modbusItem;
        _logger = logger;
    }

    #region 连接

    protected TcpClient? _tcpClient;
    public IModbusMaster? ModbusMaster { get; private set; }

    /// <summary>
    /// 创建连接并初始化
    /// </summary>
    /// <returns></returns>
    protected virtual async Task CreateConnectionAsync(int timeout)
    {
        var entered = await _connSignal.WaitAsync(timeout);
        if (!entered)
        {
            throw new TimeoutException($"ModbusTcp 通道={ChannelName} 在创建连接前，获取锁超时！");
        }
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(IpAddr, Port);
            var factory = new ModbusFactory();
            ModbusMaster = factory.CreateMaster(_tcpClient);
            ModbusMaster.Transport.ReadTimeout = ReadTimeout;
            ModbusMaster.Transport.WriteTimeout = WriteTimeout;
            _logger.LogInformation($"ModbusMaster 初始化完成: 设备名={ChannelName}; addr={IpAddr}; port={Port}");
        }
        finally
        {
            _connSignal.Release();
        }
    }

    /// <summary>
    /// 当前TCP是否已经连接
    /// </summary>
    protected virtual bool Connected
    {
        get
        {
            var s = _tcpClient?.Client;
            if (s == null)
            {
                return false;
            }
            if (!s.Connected)
            {
                return false;
            }
            // see https://stackoverflow.com/a/2661876/10091607
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = s.Available == 0;
            if (part1 && part2)
                return false;
            else
                return true;
        }
    }

    /// <summary>
    /// 确保已经建立连接
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task EnsureConnectedAsync(bool force = false)
    {
        //当前client存在并且连接有效
        if (Connected)
        {
            return;
        }
        await CreateConnectionAsync(ConnTimeout);
        return;
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task DisconnectAsync()
    {
        if (_tcpClient == null)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource<object?>();
        var th = new Thread(() =>
        {
            try
            {
                _tcpClient.Close();
                _tcpClient = null;
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("通道{ChannelName}断开连接失败：{message}\r\n{stackTrace}", ChannelName, ex.Message, ex.StackTrace);
                _tcpClient = null;
                tcs.SetException(ex);
            }
        });
        th.Start();
        return tcs.Task;
    }
    #endregion



    #region 读写

    /// <summary>
    /// 读取底层硬件，返回一段字节数组表示所读取的结果。
    /// </summary>
    /// <param name="address"></param>
    /// <param name="count"> 代表要读取的字节数。
    /// </param>
    /// <returns> 
    ///     
    ///     如果是读取HoldingRegister或者InputRegister，则返回的字节数组中每两个相邻的字节表示一个ushort；<br/>
    ///     如果是读取离散输入或者离散输出，返回字节数组，其中每个字节的含义是不为0就表示true，0表示false<br/>
    /// </returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<byte[]> ReadAsync(string address, int count)
    {

        var addr = ModBusTcpAddressParser.Parse(address);
        if (addr.Area == RegisterKinds.HoldingRegisters)
        {
            if (count % 2 != 0)
            {
                throw new Exception($"要求读取字节长度必须是偶数");
            }
            var pointsCount = (ushort)(count / 2);

            var points = await ModbusMaster!.ReadHoldingRegistersAsync(addr.SlaveAddress, addr.StartPoint, pointsCount);
            var bytes = MarshalHelper.UShortsToBytes(points);
            return bytes;
        }

        if (addr.Area == RegisterKinds.InputRegisters)
        {
            if (count % 2 != 0)
            {
                throw new Exception($"读取连续多个输入寄存器，要求读取字节长度必须是偶数");
            }
            var pointsCount = (ushort)(count / 2);

            var points = await ModbusMaster!.ReadInputRegistersAsync(addr.SlaveAddress, addr.StartPoint, pointsCount);
            var bytes = MarshalHelper.UShortsToBytes(points);
            return bytes;
        }

        if (addr.Area == RegisterKinds.InputContacts)
        {
            var pointsCount = (ushort)count;
            var flags = await ModbusMaster!.ReadInputsAsync(addr.SlaveAddress, addr.StartPoint, pointsCount);
            byte[] bytes = flags.Select(f => f ? (byte)1 : (byte)0).ToArray();
            return bytes;
        }

        if (addr.Area == RegisterKinds.OutputCoils)
        {
            var pointsCount = (ushort)count;
            var flags = await ModbusMaster!.ReadCoilsAsync(addr.SlaveAddress, addr.StartPoint, pointsCount);
            byte[] bytes = flags.Select(f => f ? (byte)1 : (byte)0).ToArray();
            return bytes;
        }

        throw new NotImplementedException();
    }


    /// <summary>
    /// 写入字节数组到底层
    /// </summary>
    /// <param name="address"></param>
    /// <param name="bytes"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public async Task WriteAsync(string address, byte[] bytes)
    {
        var addr = ModBusTcpAddressParser.Parse(address);
        if (addr.Area == RegisterKinds.HoldingRegisters)
        {
            var payload = MarshalHelper.BytesToUShorts(bytes);

            ushort offset = 0;
            while (true)
            {
                ushort currlen = (ushort)(payload.Length - offset);
                if (currlen < 0)
                {
                    throw new Exception("待写入的数据长度溢出");
                }
                if (currlen == 0)
                {
                    break;
                }
                currlen = currlen > 123 ? (ushort)123 : currlen;
                var subbytes = payload.AsSpan().Slice(offset, currlen).ToArray();

                ushort effectiveOffset = (ushort)(addr.StartPoint + offset);
                await ModbusMaster!.WriteMultipleRegistersAsync(addr.SlaveAddress, effectiveOffset, subbytes);
                offset += currlen;
            }
            return;
        }
        if (addr.Area == RegisterKinds.OutputCoils)
        {
            bool[] data = bytes.Select(b => b != 0).ToArray();
            await ModbusMaster!.WriteMultipleCoilsAsync(addr.SlaveAddress, addr.StartPoint, data);
            return;
        }

        throw new NotImplementedException();
    }
    #endregion


    public void Dispose()
    {
        if (_tcpClient != null && _tcpClient.Connected)
        {
            try
            {
                _logger.LogInformation("通道={ChannelName} 正在断开连接...", ChannelName);
                _tcpClient.Close();
                _logger.LogInformation("通道={ChannelName}  断开连接完成!", ChannelName);
            }
            catch (Exception e)
            {
                _logger.LogWarning("通道={ChannelName} 释放异常:{exception}", ChannelName, e.Message);
            }
            finally
            {

            }
        }
    }
}