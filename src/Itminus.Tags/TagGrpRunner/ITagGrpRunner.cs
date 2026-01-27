namespace Itminus.Tags;


/// <summary>
/// 通知新一轮处理
/// </summary>
/// <param name="grp"></param>
/// <param name="channel"></param>
/// <returns></returns>
public delegate Task TurnProcess(ITagGrp grp, ITagChannel channel);

/// <summary>
/// 通知新一轮轮训启动
/// </summary>
/// <param name="grp"></param>
/// <param name="channel"></param>
/// <returns></returns>
public delegate Task TurnStarted(ITagGrp grp, ITagChannel channel);

/// <summary>
/// 通知一轮错误出现
/// </summary>
/// <param name="grp"></param>
/// <param name="channel"></param>
/// <param name="ex"></param>
/// <returns></returns>
public delegate Task TurnCrashed(ITagGrp grp, ITagChannel channel, Exception ex);


/// <summary>
/// 测点群组监控
/// </summary>
public interface ITagGrpRunner
{

    /// <summary>
    /// 启动
    /// </summary>
    event TurnStarted? TurnStarted;

    /// <summary>
    /// 处理
    /// </summary>
    event TurnProcess? TurnProcess;

    /// <summary>
    /// 崩溃
    /// </summary>
    event TurnCrashed? TurnCrashed;

    /// <summary>
    /// 启动对群组的监控
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task StartAsync(ITagGrp entry, CancellationToken ct);
}