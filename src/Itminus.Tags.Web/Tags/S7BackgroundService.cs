
using Itminus.Tags;

namespace Itminus.Tags.Web.Tags;

internal class S7BackgroundService : BackgroundService
{
    private IDisposable? _dispoable;
    private readonly ITagsProject proj;

    public S7BackgroundService(ITagsProject proj)
    {
        this.proj = proj;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.proj.RunAsync(stoppingToken);
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        this._dispoable?.Dispose();
        return base.StopAsync(cancellationToken);
    }
}
