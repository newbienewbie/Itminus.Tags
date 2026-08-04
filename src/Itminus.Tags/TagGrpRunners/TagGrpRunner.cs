using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Itminus.Tags;

internal class TagGrpRunner : ITagGrpRunner
{
    private readonly ITagsProject _project;
    private readonly ILogger<TagGrpRunner> _logger;
    private readonly ITagGrpRunnerRetryStrategy _retryStrategy;
    private readonly ITagGrpRunnerPollDelayStrategy _pollDelayStrategy;
    private readonly ITagGrpRunnerDisconnectStrategy _disconnectStrategy;
    private int _consecutiveFailures;

    /// <summary>
    /// c'tor
    /// </summary>
    public TagGrpRunner(
        ITagsProject project,
        ILogger<TagGrpRunner> logger,
        ITagGrpRunnerRetryStrategy? retryStrategy = null,
        ITagGrpRunnerPollDelayStrategy? pollDelayStrategy = null,
        ITagGrpRunnerDisconnectStrategy? disconnectStrategy = null)
    {
        this._project = project;
        this._logger = logger;
        this._retryStrategy = retryStrategy ?? new DefaultTagGrpRunnerRetryStrategy();
        this._pollDelayStrategy = pollDelayStrategy ?? new AdaptivePollDelayStrategy();
        this._disconnectStrategy = disconnectStrategy ?? new DefaultTagGrpRunnerDisconnectStrategy();
    }

    /// <inheritdoc/>
    public event RunnerStarted? RunnerStarted;

    /// <inheritdoc/>
    public event TurnProcess? TurnProcess;

    /// <inheritdoc/>
    public event RunnerCrashed? RunnerCrashed;

    /// <inheritdoc/>
    public virtual async Task StartAsync(ITagGrp entry, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            ITagChannel? channel = null;
            // 本次启动是否失败？如果入口被禁用，不会被视为失败，即禁用会复位失败计数器
            var failed = false;
            try
            {
                if (!entry.IsEnabled)
                {
                    await Task.Delay(500, ct);
                    continue;
                }

                channel = entry.SearchChannel();
                if (RunnerStarted is not null)
                {
                    await RunnerStarted(entry, channel);
                }

                // 开始轮询
                var sw = new Stopwatch();
                while (!ct.IsCancellationRequested)
                {
                    sw.Restart();

                    if (channel != null)
                    {
                        await channel.EnsureConnectedAsync(force: false, ct);
                    }

                    await this.DrainWriteIntentsAsync(entry, ct);
                    if (entry.IsDirty())
                    {
                        await entry.WriteAsync(ct);
                    }
                    await entry.ReadAsync(ct);
                    if(TurnProcess is not null)
                    {
                        await TurnProcess(entry, channel);
                    }
                    await entry.WriteAsync(ct);

                    sw.Stop();
                    var span = TimeSpan.FromMilliseconds(entry.SearchScanInterval() ?? 0);
                    var delay = this._pollDelayStrategy.GetDelay(span, sw.Elapsed);
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, ct);
                    }
                    else
                    {
                        await Task.Yield();
                        ct.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (Exception ex)
            {
                failed = true;
                _consecutiveFailures++;

                try
                {
                    if (RunnerCrashed is not null)
                    {
                        try
                        {
                            await RunnerCrashed(entry, channel, ex);
                        }
                        catch(Exception handlingError)
                        {
                            this._logger.LogCritical(
                                "测点分组(分组={grp},通道={channel})错误处理又抛出了错误，这破坏了错误处理不能再抛出异常的假设。err={errMsg}\r\nStackTrace={strace}",
                                entry.TagName(),
                                channel?.ChannelName() ?? "null",
                                handlingError.Message,
                                handlingError.StackTrace
                                );
                            throw;
                        }
                    }
                }
                finally
                {
                    try
                    {
                        // 注意：这里必须使用 CancellationToken.None 而非 ct。
                        // 本 catch 块的触发原因可能是 轮询循环中的 Task.Delay(ct) 抛 OCE。
                        // 若沿用已取消的 ct，通道内部（如 S7 的 ExecuteOneByOneAsync 中 _rw.WaitAsync(ct)）会立即抛
                        // OperationCanceledException，导致 DisconnectAsync 根本没执行——连接资源泄漏，
                        // 下次 StartAsync 时旧连接可能仍处于异常状态。
                        // 断开连接是清理动作，应尽力完成，不应受取消影响。
                        //
                        // 等待策略：断开是异步的，但 PLC/Modbus 设备多有连接数限制：
                        // - Smart200/S1200 同一时刻只允许有几个连接)，
                        // - 串口设备更是只能独占。
                        // 若 fire-and-forget 后立刻重连（失败路径）或立刻重启（取消路径），
                        // 上一连接可能还没断开，导致"连接数超限"失败。因此这里委托给断开策略（默认有限超时等待），
                        // 由策略决定如何等待——正常情况毫秒级完成；超时则放弃等待、立即退出，后台仍会继续清理。
                        var disconnect = channel?.DisconnectAsync(CancellationToken.None);
                        await this._disconnectStrategy.WaitDisconnectAsync(channel, disconnect);
                    }
                    catch
                    {
                        // ignore all the error thrown by the Channel's DisconnectAsync() method
                    }
                }
            }
            finally
            {
                // 成功迭代则重置连续失败计数器；失败则使用策略等待
                if (!failed)
                {
                    _consecutiveFailures = 0;
                }

                var delay = _consecutiveFailures > 0
                    ? _retryStrategy.GetDelay(_consecutiveFailures)
                    : TimeSpan.FromMilliseconds(entry.SearchScanInterval() ?? 0);
                await Task.Delay(delay, ct);
            }
        }
    }

    /// <summary>
    /// 处理写入意图队列，直到队列为空或者达到容量上限。<br/>
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected virtual async Task DrainWriteIntentsAsync(ITagGrp entry, CancellationToken ct)
    {
        var reader = this._project.GetIntentReader(entry.TagName());
        if(reader is null)
        {
            return;
        }

        var count = 0;
        while (reader.TryRead(out var item))
        {
            var intent = item.Intent;
            var tcs = item.Completion;
            try
            {
                await intent(entry, ct);
                tcs.TrySetResult();
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                tcs.TrySetCanceled(ct);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            count++;
            if (count >= this._project.IntentCapacity)
            {
                break;
            }
        }
    }
}
