using Microsoft.Extensions.Logging;

namespace Itminus.Tags;

internal class TagGrpRunner : ITagGrpRunner
{
    private readonly ITagsProject _project;
    private readonly ILogger<TagGrpRunner> _logger;

    public TagGrpRunner(ITagsProject project, ILogger<TagGrpRunner> logger)
    {
        this._project = project;
        this._logger = logger;
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
            try
            {
                channel = entry.GetChannel();
                if (!entry.IsEnabled)
                {
                    await Task.Delay(500, ct);
                    continue;
                }

                if (TurnStarted is not null)
                {
                    await TurnStarted(entry, channel);
                }

                // 开始轮询
                while (!ct.IsCancellationRequested)
                {
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
                    await Task.Delay(entry.ScanInterval, ct);
                }
            }
            catch (Exception ex)
            {

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
                await Task.Delay(entry.ScanInterval, ct);
            }
        }
    }

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
