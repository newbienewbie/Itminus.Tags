
namespace Itminus.Tags.Web.Tags;

internal class S7BackgroundService : BackgroundService
{
    private readonly S7TagRuntime_Rx _rt;
    private IDisposable? _dispoable;

    public S7BackgroundService(S7TagRuntime_Rx rt)
    {
        this._rt = rt;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this._dispoable = this._rt.LaunchObservable().Subscribe();
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        this._dispoable?.Dispose();
        return base.StopAsync(cancellationToken);
    }
}
