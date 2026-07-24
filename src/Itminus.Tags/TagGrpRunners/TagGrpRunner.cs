using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Itminus.Tags;

internal class TagGrpRunner : ITagGrpRunner
{
    private readonly ITagsProject _project;
    private readonly ILogger<TagGrpRunner> _logger;
    private readonly ITagGrpRunnerRetryStrategy _retryStrategy;
    private readonly ITagGrpRunnerPollDelayStrategy _pollDelayStrategy;
    private int _consecutiveFailures;

    /// <summary>
    /// c'tor
    /// </summary>
    public TagGrpRunner(
        ITagsProject project,
        ILogger<TagGrpRunner> logger,
        ITagGrpRunnerRetryStrategy? retryStrategy = null,
        ITagGrpRunnerPollDelayStrategy? pollDelayStrategy = null)
    {
        this._project = project;
        this._logger = logger;
        this._retryStrategy = retryStrategy ?? new DefaultTagGrpRunnerRetryStrategy();
        this._pollDelayStrategy = pollDelayStrategy ?? new AdaptivePollDelayStrategy();
    }

    /// <inheritdoc/>
    public event TurnStarted? TurnStarted;

    /// <inheritdoc/>
    public event TurnProcess? TurnProcess;

    /// <inheritdoc/>
    public event TurnCrashed? TurnCrashed;

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
                if (TurnStarted is not null)
                {
                    await TurnStarted(entry, channel);
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
                    var delay = this._pollDelayStrategy.GetDelay(TimeSpan.FromMilliseconds(entry.ScanInterval), sw.Elapsed);
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
                    if (TurnCrashed is not null)
                    {
                        try
                        {
                            await TurnCrashed(entry, channel, ex);
                        }
                        catch(Exception handlingError)
                        {
                            this._logger.LogCritical(
                                "测点分组(分组={grp},通道={channel})错误处理又抛出了错误，这破坏了错误处理不能再抛出异常的假设。err={errMsg}\r\nStackTrace={strace}",
                                entry.Name,
                                channel?.ChannelName ?? "null",
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
                        channel?.DisconnectAsync(ct);
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
                    : TimeSpan.FromMilliseconds(entry.ScanInterval);
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
        var reader = this._project.GetIntentReader(entry.Name);
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
