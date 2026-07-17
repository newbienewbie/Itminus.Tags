using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using WpfDemo.Tags;
using WpfDemo.Tags.Logicets;

namespace WpfDemo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider? Root { get; private set; }
    internal TagsProjectCtrl? Ctrl { get; private set; }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        ServiceCollection services = ConfigureServiceCollections();
        this.Root = services.BuildServiceProvider();
        this.Ctrl = this.Root.GetRequiredService<TagsProjectCtrl>();
        TimerResolution.TimeBeginPeriod(1);
        var th = new Thread(async () =>
        {
            var loggerFactory = this.Root.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<App>();

            var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            await this.Ctrl.StartPollAsync(Path.Combine(dir!.FullName, "Tags"), (proj, ct) =>
            {
                proj.Logicets.Add(new HeartBeatLogicet(
                    proj.Channels,
                    proj.Tags,
                    loggerFactory.CreateLogger<HeartBeatLogicet>()
                ));

                //proj.TurnStarted += (grp, ch) => {
                //    logger.LogInformation("Tags处理开始,grp={grpName}", grp.Name);
                //    return Task.CompletedTask;
                //};
                proj.TurnCrashed += (grp, ch, ex) => {
                    logger.LogError(ex, "Tags处理异常,grp={grp.Name}", grp.Name);
                    return Task.CompletedTask;
                };

                return Task.CompletedTask;
            });
        });
        th.IsBackground = true;
        th.Start();

    }

    private static ServiceCollection ConfigureServiceCollections()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        //services.AddSerilog((sp, lc) => lc
        //    .ReadFrom.Services(sp)
        //    .WriteTo.Console()
        //    .WriteTo.File("logs/log.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
        //    .Enrich.FromLogContext()
        //);
        services.AddWpfDemoTags();
        return services;
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        TimerResolution.TimeEndPeriod(1);
        if (this.Ctrl is not null)
        {
            await this.Ctrl.StopAsync();
        }
        Environment.Exit(0);
    }
}
