namespace Itminus.Tags;

public class TagGrpRunner : ITagGrpRunner
{
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
            var channel = entry.GetRequiredChannel();
            try
            {
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
                    await channel.EnsureConnectedAsync(force: false, ct);
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
                        await TurnCrashed(entry, channel, ex);
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
}
