namespace Itminus.Tags;

/// <summary>
/// 测点群组运行器（<see cref="ITagGrpRunner"/>）的轮询间隔等待策略。<br/>
/// 控制内层轮询循环中每轮迭代之间的等待方式。
/// </summary>
public interface ITagGrpRunnerPollDelayStrategy
{
    /// <summary>
    /// 根据扫描间隔和本轮实际耗时，执行下一次轮询前的等待。
    /// </summary>
    /// <param name="scanInterval">配置的扫描间隔</param>
    /// <param name="elapsed">本轮迭代的实际耗时</param>
    /// <param name="ct">取消令牌</param>
    Task DelayAsync(TimeSpan scanInterval, TimeSpan elapsed, CancellationToken ct);
}
