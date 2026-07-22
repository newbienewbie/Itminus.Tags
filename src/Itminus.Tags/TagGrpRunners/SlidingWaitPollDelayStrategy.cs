namespace Itminus.Tags;

/// <summary>
/// 固定滑动等待策略：每轮迭代后始终等待固定的时长。<br/>
/// 这是 <see cref="TagGrpRunner"/> 原有的行为。
/// </summary>
public class SlidingWaitPollDelayStrategy : ITagGrpRunnerPollDelayStrategy
{
    /// <inheritdoc/>
    public Task DelayAsync(TimeSpan scanInterval, TimeSpan elapsed, CancellationToken ct)
        => Task.Delay(scanInterval, ct);
}
