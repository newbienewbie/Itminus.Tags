namespace Itminus.Tags;

/// <summary>
/// 采集通道
/// </summary>
public interface ITagChannel : IDisposable
{
    /// <summary>
    /// 通道名称
    /// </summary>
    string ChannelName { get; }

    /// <summary>
    /// 驱动
    /// </summary>
    string Driver { get; }

    /// <summary>
    /// 连接
    /// </summary>
    /// <returns></returns>
    Task EnsureConnectedAsync(bool force = false);

    /// <summary>
    /// 关闭
    /// </summary>
    /// <returns></returns>
    Task DisconnectAsync();

    /// <summary>
    /// 读取底层硬件，返回一段字节数组表示所读取的结果。
    /// </summary>
    /// <param name="address"></param>
    /// <param name="count></param>
    /// <returns></returns>
    public Task<byte[]> ReadAsync(string address, int count);

    /// <summary>
    /// 写入字节数组
    /// </summary>
    /// <param name="address"></param>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public Task WriteAsync(string address, byte[] bytes);
}
