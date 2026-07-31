using Itminus.Tags;
using System.IO;
using System.Reflection;
using System.Windows;
using WpfDemo.Tags.Logicets;

namespace WpfDemo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider? Root { get; private set; }
    internal ITagsProjectCtrl? Ctrl { get; private set; }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.ConfigureServies();
        var app = builder.Build();
        this.Root = app.Services;
        this.Ctrl = this.Root.GetRequiredService<ITagsProjectCtrl>();
        TimerResolution.TimeBeginPeriod(1);

        StartTagsPoll(this.Root, this.Ctrl);
        StartWeb(app);
    }

    private void StartTagsPoll(IServiceProvider sp, ITagsProjectCtrl ctrl)
    {
        var th1 = new Thread(async () =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<App>();

            var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            await ctrl.StartPollAsync(Path.Combine(dir!.FullName, "Tags"), null, (proj, sp, ct) =>
            {
                proj.Logicets.Add(new HeartBeatLogicet(
                    proj.Channels,
                    proj.Tags,
                    loggerFactory.CreateLogger<HeartBeatLogicet>()
                ));

                proj.TurnStarted += (grp, ch) =>
                {
                    logger.LogInformation("Tags处理开始,grp={grpName}", grp.TagName());
                    return Task.CompletedTask;
                };
                proj.TurnCrashed += (grp, ch, ex) =>
                {
                    logger.LogError(ex, "Tags处理异常,grp={grpName}", grp.TagName());
                    MessageBox.Show($"Tags处理发生异常：{ex.Message}");
                    return Task.CompletedTask;
                };

                return Task.CompletedTask;
            });
        });
        th1.IsBackground = true;
        th1.Start();
    }

    private static void StartWeb(WebApplication app)
    {
        app.ConfigureMiddlewares();
        var th2 = new Thread(() =>
        {
            app.Run("http://localhost:3001");
        });
        th2.IsBackground = true;
        th2.Start();
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
