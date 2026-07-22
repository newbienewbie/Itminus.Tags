using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.TagGrpRunners;

public class SlidingWaitPollDelayStrategyTests
{
    [Fact]
    public async Task SlidingWait_AlwaysWaitsFullInterval()
    {
        // Arrange
        var strategy = new SlidingWaitPollDelayStrategy();
        var ct = CancellationToken.None;

        // Act — measure actual wait time
        var sw = Stopwatch.StartNew();
        await strategy.DelayAsync(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(10), ct);
        sw.Stop();

        // Assert — should have waited approximately 50ms
        Assert.True(sw.ElapsedMilliseconds >= 40,
            $"实际等待 {sw.ElapsedMilliseconds}ms 应 >= 40ms");
    }
}
