namespace Itminus.Tags;

/// <summary>
/// 自适应等待策略：<br/>
/// - 如果本轮耗时已超过扫描间隔，则仅让出调度，不额外等待；<br/>
/// - 否则等待剩余时间（<c>scanInterval - elapsed</c>）。<br/>
/// 这样在高负载或 IO 延迟波动时不会累积排队延迟。
/// </summary>
public class AdaptivePollDelayStrategy : ITagGrpRunnerPollDelayStrategy
{
    /// <inheritdoc/>
    public async Task DelayAsync(TimeSpan scanInterval, TimeSpan elapsed, CancellationToken ct)
    {
        // 如果已超时或刚好用完时间，让出调度即可
        if (elapsed >= scanInterval)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();
            return;
        }

        var remaining = scanInterval - elapsed;
        await Task.Delay(remaining, ct);
    }
}
