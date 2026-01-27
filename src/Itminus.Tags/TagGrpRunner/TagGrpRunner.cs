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
    public virtual async Task StartAsync(ITagGrp grp, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var channel = grp.GetRequiredChannel();
            try
            {
                if (!grp.IsEnabled)
                {
                    await Task.Delay(500);
                    continue;
                }

                if (TurnStarted is not null)
                {
                    await TurnStarted(grp, channel);
                }

                // 开始轮询
                while (!ct.IsCancellationRequested)
                {
                    await channel.EnsureConnectedAsync();
                    await grp.ReadAsync(ct);
                    if(TurnProcess is not null)
                    {
                        await TurnProcess(grp, channel);
                    }
                    await grp.WriteAsync(ct);
                    await Task.Delay(grp.ScanInterval, ct);
                }
            }
            catch (Exception ex)
            {

                try
                {
                    if (TurnCrashed is not null)
                    {
                        await TurnCrashed(grp, channel, ex);
                    }
                    try
                    {
                        channel?.DisconnectAsync();
                    }
                    catch 
                    { 
                    }
                }
                catch
                {

                }
            }
            finally
            {
                await Task.Delay(grp.ScanInterval, ct);
            }
        }
    }
}
