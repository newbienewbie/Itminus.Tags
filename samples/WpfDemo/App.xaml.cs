using Itminus.Tags;
using Itminus.Tags.BlazorLib;
using Itminus.Tags.BlazorLib.Pages;
using Itminus.Tags.McpServer;
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
    internal ITagsProjectCtrl? Ctrl { get; private set; }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var builder = WebApplication.CreateBuilder();
        ConfigureServiceCollections(builder.Services);
        var app = builder.Build();
        this.Root = app.Services;
        this.Ctrl = this.Root.GetRequiredService<ITagsProjectCtrl>();
        TimerResolution.TimeBeginPeriod(1);

        StartTagsPoll(this.Root, this.Ctrl);
        StartWeb(app);
    }


    private static void ConfigureServiceCollections(IServiceCollection services)
    {
        services.AddLogging();
        //services.AddSerilog((sp, lc) => lc
        //    .ReadFrom.Services(sp)
        //    .WriteTo.Console()
        //    .WriteTo.File("logs/log.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
        //    .Enrich.FromLogContext()
        //);
        services.AddAntiforgery();
        services.AddWpfDemoTags();
        
        services.AddMcpServer()
            .WithHttpTransport(opts => {
                opts.Stateless = true;
            })
            .AddTagsMcp();

        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        services.AddTagsBlazorLibCore();
        services.AddAuthentication();
    }

    private void StartTagsPoll(IServiceProvider sp, ITagsProjectCtrl ctrl)
    {
        var th1 = new Thread(async () =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<App>();

            var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            await ctrl.StartPollAsync(Path.Combine(dir!.FullName, "Tags"), null, (proj, ct) =>
            {
                proj.Logicets.Add(new HeartBeatLogicet(
                    proj.Channels,
                    proj.Tags,
                    loggerFactory.CreateLogger<HeartBeatLogicet>()
                ));

                proj.TurnStarted += (grp, ch) =>
                {
                    logger.LogInformation("Tags处理开始,grp={grpName}", grp.Name);
                    return Task.CompletedTask;
                };
                proj.TurnCrashed += (grp, ch, ex) =>
                {
                    logger.LogError(ex, "Tags处理异常,grp={grp.Name}", grp.Name);
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
        var th2 = new Thread(() =>
        {
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAntiforgery();
            app.MapRazorComponents<Itminus.Tags.BlazorLib.Apps.TagsApp>()
                .AddInteractiveServerRenderMode();
            app.MapMcp("/mcp");
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
