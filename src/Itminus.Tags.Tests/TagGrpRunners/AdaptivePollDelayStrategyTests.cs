using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.TagGrpRunners;

public class AdaptivePollDelayStrategyTests
{
    [Fact]
    public async Task Adaptive_WhenUnderInterval_WaitsRemaining()
    {
        // Arrange — elapsed=10ms, interval=50ms, 期待等待 ~40ms
        var strategy = new AdaptivePollDelayStrategy();
        var ct = CancellationToken.None;

        // Act
        var sw = Stopwatch.StartNew();
        await strategy.DelayAsync(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(10), ct);
        sw.Stop();

        // Assert — 应等待剩余时间（约 40ms）
        Assert.True(sw.ElapsedMilliseconds >= 30,
            $"实际等待 {sw.ElapsedMilliseconds}ms 应 >= 30ms");
    }

    [Fact]
    public async Task Adaptive_WhenOverInterval_DoesNotWait()
    {
        // Arrange — elapsed=60ms > interval=50ms, 期待不等待（仅 Yield）
        var strategy = new AdaptivePollDelayStrategy();
        var ct = CancellationToken.None;

        // Act
        var sw = Stopwatch.StartNew();
        await strategy.DelayAsync(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(60), ct);
        sw.Stop();

        // Assert — Yield 几乎瞬间返回
        Assert.True(sw.ElapsedMilliseconds < 40,
            $"超时时等待 {sw.ElapsedMilliseconds}ms 应接近 0");
    }



    [Fact]
    public async Task Adaptive_PropagatesCancellation()
    {
        // Arrange
        var strategy = new AdaptivePollDelayStrategy();
        using var cts = new CancellationTokenSource();

        // Act & Assert — 等待剩余时间时取消
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            strategy.DelayAsync(TimeSpan.FromMilliseconds(1000), TimeSpan.Zero, cts.Token));
    }
}
