
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

}
