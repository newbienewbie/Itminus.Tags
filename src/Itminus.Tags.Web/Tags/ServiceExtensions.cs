
using System.Reflection;
using Itminus.Tags.S7;
using Itminus.Tags.ZLan;
using Itminus.Tags.ModbusTcp;
using Itminus.Tags.OpcUaClient;
using Itminus.Tags.ComScanner;
using Itminus.Tags.Hjzk;
using System.Xml.Linq;

namespace Itminus.Tags.Web.Tags;

public static class ServiceExtensions
{
    public static void AddTags(this IServiceCollection services)
    {
        services.AddTagsProjectServices(b =>
        {
            b.AddS7Support();
            b.AddModbusTcpSupport();
            b.AddZLanTcpSupport();
            b.AddOpcUaClientSupport();
            b.AddHjzkSupport();
            b.AddComScannerSupport();
        });

        services.AddSingleton<TagsProjectCtrl>();
    }

    public static ITagsProject MakeProject(this IServiceProvider sp, string? dir=null, XElement? root=null)
    {
        var factory = sp.GetRequiredService<ITagsProjectFactory>();

        if (string.IsNullOrEmpty(dir)) 
        {
            var loc = Assembly.GetExecutingAssembly().Location;
            dir = Path.GetDirectoryName(loc);
        }
        if (string.IsNullOrEmpty(dir))
        {
            dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        var proj = factory.Create(dir!, root);

        proj.TurnStarted += (grp, ch) => {
            Console.WriteLine($"[Tags] 开始处理分组 {grp.Name}");
            return Task.CompletedTask;
        };
        proj.TurnCrashed += (grp, ch, ex) => {
            Console.WriteLine("{0}: 轮次错误：{1}", grp.Name, ex.Message);
            return Task.CompletedTask;
        };


        return proj;
    }
}
