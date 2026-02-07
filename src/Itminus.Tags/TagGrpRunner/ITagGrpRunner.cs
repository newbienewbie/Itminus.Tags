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
/// 测点群组运行器。<br/>
/// 这个运行期运行机制类似于一个PLC。它遵循:<br/>
///     <b>读取输入</b>-&gt;<b>处理</b>-&gt;<b>输出</b> 
/// 的循环。<br/>
/// 一旦成功启动循环，除非主动取消或者被异常处理的二次异常打断，否则不会停止。
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
    /// 崩溃处理。<br/>
    /// 注意崩溃处理中不要再有异常发生，否则会打断轮询
    /// </summary>
    event TurnCrashed? TurnCrashed;

    /// <summary>
    /// 启动对群组的监控: loop(输入 -> 处理 ->输出)<br/>
    /// 一旦成功启动轮询，除非主动取消，否则不会抛出异常。
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task StartAsync(ITagGrp entry, CancellationToken ct);
}