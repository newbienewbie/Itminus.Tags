using System.Reflection;
using Itminus.Tags;

namespace NixMonitor.Tags;

class NixMonitorBackgroundService: BackgroundService
{
    private readonly ILogger<NixMonitorBackgroundService> _logger;

    private readonly ITagsProjectCtrl _ctrl;

    public NixMonitorBackgroundService(ITagsProjectCtrl ctrl, ILogger<NixMonitorBackgroundService> logger)
    {
        this._ctrl = ctrl;
        this._logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
        stoppingToken.Register(async () =>
        {
            _logger.LogInformation("Tags处理停止");
            await _ctrl.StopAsync();
        });
        await _ctrl.StartPollAsync(Path.Combine(dir!.FullName, "Tags"), null, (proj, sp, ct) =>
        {
            proj.RunnerStarted += (grp, ch) =>
            {
                _logger.LogInformation("Tags处理开始,grp={grpName}", grp.TagName());
                return Task.CompletedTask;
            };
            proj.RunnerCrashed += (grp, ch, ex) =>
            {
                _logger.LogError(ex, "Tags处理异常,grp={grpName}", grp.TagName());
                return Task.CompletedTask;
            };

            return Task.CompletedTask;
        });
    }
}
