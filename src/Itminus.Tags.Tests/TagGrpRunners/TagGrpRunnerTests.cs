using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using Itminus.Tags.Tests.Fakes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Itminus.Tags.Tests.TagGrpRunners;

public class TagGrpRunnerTests
{
    /// <summary>
    /// 创建一个 TagGrpRunner + 其依赖，所有 mock 均为公开可访问状态。
    /// </summary>
    private (TagGrpRunner runner, MockTagGrp entry, MockProject project, FakedChannel channel) CreateRunner()
    {
        var channel = new FakedChannel();
        var entry = new MockTagGrp { Channel = channel, ScanInterval = 10_000 };
        var project = new MockProject();
        var runner = new TagGrpRunner(project, NullLogger<TagGrpRunner>.Instance);
        return (runner, entry, project, channel);
    }

    /// <summary>
    /// 调用 StartAsync 并在取消时吞掉 OperationCanceledException。
    /// TagGrpRunner 的外层 finally 中有 Task.Delay(ScanInterval, ct)，
    /// 取消后会抛出 OCE，这是其当前设计。
    /// </summary>
    private async Task RunUntilCancelled(TagGrpRunner runner, ITagGrp entry, CancellationToken ct)
    {
        try
        {
            await runner.StartAsync(entry, ct);
        }
        catch (OperationCanceledException)
        {
            // expected on cancellation
        }
    }

    [Fact]
    public async Task StartAsync_NormalLoop_CallsReadWriteAndEvents()
    {
        // Arrange
        var (runner, entry, _, _) = CreateRunner();
        entry.ScanInterval = 20;
        entry.IsEnabled = true;

        using var cts = new CancellationTokenSource();
        var turnStartedCalled = false;
        var turnProcessCalled = false;

        runner.TurnStarted += (grp, ch) =>
        {
            turnStartedCalled = true;
            return Task.CompletedTask;
        };
        runner.TurnProcess += (grp, ch) =>
        {
            turnProcessCalled = true;
            cts.Cancel();
            return Task.CompletedTask;
        };

        // Act
        await RunUntilCancelled(runner, entry, cts.Token);

        // Assert
        Assert.True(turnStartedCalled, "TurnStarted 应被触发");
        Assert.True(turnProcessCalled, "TurnProcess 应被触发");
        Assert.True(entry.ReadAsyncCallCount >= 1, "ReadAsync 应被调用至少1次");
        Assert.True(entry.WriteAsyncCallCount >= 1, "WriteAsync 应被调用至少1次");
    }

    [Fact]
    public async Task StartAsync_WhenDisabled_SkipsInnerLoop()
    {
        // Arrange
        var (runner, entry, _, _) = CreateRunner();
        entry.ScanInterval = 500;
        entry.IsEnabled = false;

        using var cts = new CancellationTokenSource(800);

        // Act
        await RunUntilCancelled(runner, entry, cts.Token);

        // Assert
        Assert.Equal(0, entry.ReadAsyncCallCount);
        Assert.Equal(0, entry.WriteAsyncCallCount);
    }

    [Fact]
    public async Task StartAsync_TurnStartedEvent_Fires()
    {
        // Arrange
        var (runner, entry, _, _) = CreateRunner();
        entry.ScanInterval = 20;
        entry.IsEnabled = true;

        using var cts = new CancellationTokenSource();
        ITagGrp? capturedEntry = null;
        ITagChannel? capturedChannel = null;

        runner.TurnStarted += (grp, ch) =>
        {
            capturedEntry = grp;
            capturedChannel = ch;
            cts.Cancel();
            return Task.CompletedTask;
        };

        // Act
        await RunUntilCancelled(runner, entry, cts.Token);

        // Assert
        Assert.Same(entry, capturedEntry);
        Assert.Same(entry.Channel, capturedChannel);
    }

    [Fact]
    public async Task StartAsync_TurnProcessEvent_FiresAfterRead()
    {
        // Arrange
        var (runner, entry, _, _) = CreateRunner();
        entry.ScanInterval = 20;
        entry.IsEnabled = true;

        using var cts = new CancellationTokenSource();
        var processOrder = new List<string>();

        entry.OnRead = () => processOrder.Add("Read");
        entry.OnWrite = () => processOrder.Add("Write");

        runner.TurnProcess += (grp, ch) =>
        {
            processOrder.Add("Process");
            cts.Cancel();
            return Task.CompletedTask;
        };

        // Act
        await RunUntilCancelled(runner, entry, cts.Token);

        // Assert
        Assert.Contains("Read", processOrder);
        Assert.Contains("Process", processOrder);
        Assert.Contains("Write", processOrder);
        var readIdx = processOrder.IndexOf("Read");
        var writeIdx = processOrder.IndexOf("Write");
        Assert.True(readIdx < writeIdx, "Read 应在 Write 之前");
    }

    [Fact]
    public async Task StartAsync_IsDirty_CallsWriteBeforeRead()
    {
        // Arrange
        var (runner, entry, _, _) = CreateRunner();
        entry.ScanInterval = 20;
        entry.IsEnabled = true;
        entry.IsDirtyReturn = true;

        using var cts = new CancellationTokenSource();
        var callOrder = new List<string>();

        entry.OnWrite = () => callOrder.Add("Write");
        entry.OnRead = () => callOrder.Add("Read");

        runner.TurnProcess += (_, _) =>
        {
            cts.Cancel();
            return Task.CompletedTask;
        };

        // Act
        await RunUntilCancelled(runner, entry, cts.Token);

        // Assert
        var writeIdx = callOrder.IndexOf("Write");
        var readIdx = callOrder.IndexOf("Read");
        Assert.True(writeIdx >= 0, "至少有一次 Write");
        Assert.True(readIdx >= 0, "至少有一次 Read");
        Assert.True(writeIdx < readIdx, "IsDirty 时应在 Read 之前先 Write");
    }

    [Fact]
    public async Task StartAsync_Exception_FiresTurnCrashed()
    {
        // Arrange
        var (runner, entry, _, _) = CreateRunner();
        entry.ScanInterval = 20;
        entry.IsEnabled = true;
        entry.ReadAsyncThrows = new InvalidOperationException("模拟读取异常");

        using var cts = new CancellationTokenSource(2000);
        Exception? capturedEx = null;
        var crashedFired = false;

        runner.TurnCrashed += (grp, ch, ex) =>
        {
            crashedFired = true;
            capturedEx = ex;
            cts.Cancel();
            return Task.CompletedTask;
        };

        // Act
        await RunUntilCancelled(runner, entry, cts.Token);

        // Assert
        Assert.True(crashedFired, "TurnCrashed 应被触发");
        Assert.NotNull(capturedEx);
        Assert.IsType<InvalidOperationException>(capturedEx);
        Assert.Contains("模拟读取异常", capturedEx.Message);
    }

    [Fact]
    public async Task StartAsync_TurnCrashedHandlerThrows_ExceptionPropagates()
    {
        // Arrange
        var (runner, entry, _, _) = CreateRunner();
        entry.ScanInterval = 20;
        entry.IsEnabled = true;
        entry.ReadAsyncThrows = new InvalidOperationException("模拟读取异常");

        using var cts = new CancellationTokenSource(2000);

        runner.TurnCrashed += (grp, ch, ex) =>
            throw new InvalidOperationException("错误处理也抛异常");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runner.StartAsync(entry, cts.Token));

        Assert.Contains("错误处理也抛异常", ex.Message);
    }

    [Fact]
    public async Task StartAsync_Cancellation_StopsLoop()
    {
        // Arrange
        var (runner, entry, _, _) = CreateRunner();
        entry.ScanInterval = 10;
        entry.IsEnabled = true;

        using var cts = new CancellationTokenSource(100);

        // Act
        await RunUntilCancelled(runner, entry, cts.Token);

        // Assert
        Assert.True(entry.ReadAsyncCallCount >= 1, "取消前应至少执行了一次 ReadAsync");
    }

    [Fact]
    public async Task StartAsync_ProcessIntents()
    {
        // Arrange
        var entry = new MockTagGrp
        {
            Name = "intent-entry",
            Channel = new FakedChannel(),
            ScanInterval = 20,
            IsEnabled = true
        };
        var project = new MockProject();
        var runner = new TagGrpRunner(project, NullLogger<TagGrpRunner>.Instance);

        using var cts = new CancellationTokenSource();
        var intentExecuted = false;

        project.WriteIntent("intent-entry", (grp, ct) =>
        {
            intentExecuted = true;
            return ValueTask.CompletedTask;
        }, out var intentTask);

        runner.TurnProcess += (_, _) =>
        {
            cts.Cancel();
            return Task.CompletedTask;
        };

        // Act
        await RunUntilCancelled(runner, entry, cts.Token);

        // Assert — 意图已被 DrainWriteIntentsAsync 处理，intentTask 应完成
        Assert.True(intentExecuted, "意图应被执行");
        Assert.True(intentTask.IsCompletedSuccessfully, "intentTask 应成功完成");
    }



    #region Mocks

    private class MockTagGrp : ITagGrp
    {
        public string Name { get; set; } = "test-entry";
        public ITagGrp? Parent { get; set; }
        public bool IsEntry { get; } = true;
        public IDictionary<string, TagUnion> Children { get; } = new Dictionary<string, TagUnion>();
        public TagUnion this[string tagName] => throw new NotImplementedException();
        public TagUnion Descendant(string path) => throw new NotImplementedException();
        public ITagGrp AddTag(ITag tag) => this;
        public ITagGrp AddTag(ITagCbnt tagCbnt) => this;
        public ITagGrp AddTag(ITagGrp tagGrp) => this;
        public int ScanInterval { get; set; } = 1000;
        public bool IsEnabled { get; set; } = true;
        public ITagChannel? Channel { get; set; }

        public int ReadAsyncCallCount { get; private set; }
        public int WriteAsyncCallCount { get; private set; }
        public Exception? ReadAsyncThrows { get; set; }
        public bool IsDirtyReturn { get; set; }
        public Action? OnRead { get; set; }
        public Action? OnWrite { get; set; }

        public Task ReadAsync(CancellationToken ct)
        {
            ReadAsyncCallCount++;
            OnRead?.Invoke();
            if (ReadAsyncThrows is not null)
                throw ReadAsyncThrows;
            return Task.CompletedTask;
        }

        public Task WriteAsync(CancellationToken ct)
        {
            WriteAsyncCallCount++;
            OnWrite?.Invoke();
            return Task.CompletedTask;
        }

        public bool IsDirty() => IsDirtyReturn;
    }

    private class MockProject : ITagsProject
    {
        public IReadOnlyList<ITagChannel> Channels => Array.Empty<ITagChannel>();
        public IList<ILogicet> Logicets => new List<ILogicet>();
        public ITagGrp Tags => null!;
        public string? ProjectRoot { get; set; }
        public int IntentCapacity { get; set; } = 10;

        private readonly ConcurrentDictionary<string, Channel<IntentCompletion>> _channels = new();

        public ChannelReader<IntentCompletion>? GetIntentReader(string entry) =>
            _channels.TryGetValue(entry, out var ch) ? ch.Reader : null;

        public bool WriteIntent(string entry, TagGrpWriteIntent intent, out Task task)
        {
            var ch = _channels.GetOrAdd(entry, _ => CreateIntentChannel());
            var writer = ch.Writer;
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var item = new IntentCompletion(intent, tcs);
            task = tcs.Task;
            var written = writer.TryWrite(item);
            if (!written)
                tcs.TrySetException(new Exception($"写入意图失败: {entry}"));
            return written;
        }

        public bool WriteIntent(string entry, TagGrpWriteIntent intent) =>
            WriteIntent(entry, intent, out _);

        private Channel<IntentCompletion> CreateIntentChannel()
        {
            var capacity = IntentCapacity > 0 ? IntentCapacity : 10;
            return Channel.CreateBounded<IntentCompletion>(new BoundedChannelOptions(capacity)
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
                FullMode = BoundedChannelFullMode.Wait,
            });
        }

        public XElement? GetRootElement() => null;
        public void Initialize(string projRoot, XElement? root = null) { }
        public void Dispose() { }
        public Task RunAsync(CancellationToken ct) => Task.CompletedTask;


        public event TurnStarted? TurnStarted;
        public event TurnCrashed? TurnCrashed;
    }

    #endregion
}
