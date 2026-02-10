
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Itminus.Tags;
using Itminus.Tags.S7;
using Itminus.Tags.ZLan;
using Itminus.Tags.ModbusTcp;
using Itminus.Tags.OpcUaClient;
using Itminus.Tags.ComScanner;

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

            b.AddComScannerSupport();
        });

        services.AddSingleton<TagsProjectCtrl>();
    }

    public static ITagsProject MakeProject(this IServiceProvider sp, string? dir=null)
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

        var proj = factory.Create(dir!);

        proj.TurnStarted += (grp, ch) => {
            Console.WriteLine($"[Tags] 开始处理分组 {grp.Name}");
            return Task.CompletedTask;
        };
        proj.TurnCrashed += (grp, ch, ex) => {
            Console.WriteLine("{0}: 轮次错误：{1}", grp.Name, ex.Message);
            return Task.CompletedTask;
        };


        proj.Logicets.Clear();
        proj.TryAddLogicet<HandleSnap11>();
        proj.TryAddLogicet<HandleSnap12>();
        proj.TryAddLogicet<HandleSnap13>();
        proj.TryAddLogicet<HandleSnap14>();


        return proj;
    }
}
