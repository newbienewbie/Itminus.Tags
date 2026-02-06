
using Itminus.Tags;

namespace Itminus.Tags.Web.Tags;

internal class TagProjBackgroundService : BackgroundService
{
    private readonly ITagsProject proj;
    private readonly ILogger<TagProjBackgroundService> _logger;

    public TagProjBackgroundService(ITagsProject proj, ILogger<TagProjBackgroundService> logger)
    {
        this.proj = proj;
        this._logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(() => this.proj.RunAsync(stoppingToken));

        var tag = this.proj.Tags.SelectTag("扫码枪/输入");
        tag.ObserveOnRead().Subscribe(evt => {
            this._logger.LogInformation($"----------{evt.EventArgs.NewValue}");
        });
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}
