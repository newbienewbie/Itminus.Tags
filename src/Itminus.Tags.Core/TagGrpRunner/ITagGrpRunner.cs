using System.Threading.Channels;

namespace Itminus.Tags;


/// <summary>
/// <see cref="ITagGrpRunner"/>中有一轮处理
/// </summary>
/// <param name="grp"></param>
/// <param name="channel"></param>
/// <returns></returns>
public delegate Task TurnProcess(ITagGrp grp, ITagChannel? channel);

/// <summary>
/// <see cref="ITagGrpRunner"/>中有轮训启动
/// </summary>
/// <param name="grp"></param>
/// <param name="channel"></param>
/// <returns></returns>
public delegate Task RunnerStarted(ITagGrp grp, ITagChannel? channel);

/// <summary>
/// <see cref="ITagGrpRunner"/>中有错误出现
/// </summary>
/// <param name="grp"></param>
/// <param name="channel"></param>
/// <param name="ex"></param>
/// <returns></returns>
public delegate Task RunnerCrashed(ITagGrp grp, ITagChannel? channel, Exception ex);


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
    /// 启动事件
    /// </summary>
    event RunnerStarted? RunnerStarted;

    /// <summary>
    /// 崩溃事件。<br/>
    /// 注意这里不要再有异常发生，否则会视作对应的入口轮询需要停机
    /// </summary>
    event RunnerCrashed? RunnerCrashed;

    /// <summary>
    /// 每一轮处理
    /// </summary>
    event TurnProcess? TurnProcess;

    /// <summary>
    /// 启动对群组的监控: loop(意图执行-> 读取输入 -> 逻辑处理 -> 刷写输出)<br/>
    /// 一旦成功启动轮询，除非主动取消，否则不会抛出异常。
    /// </summary>
    /// <param name="entry">入口</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task StartAsync(ITagGrp entry, CancellationToken ct);
}