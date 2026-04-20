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
    Task EnsureConnectedAsync(bool force, CancellationToken ct);

    /// <summary>
    /// 关闭
    /// </summary>
    /// <returns></returns>
    Task DisconnectAsync(CancellationToken ct);

}
