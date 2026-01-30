using Itminus.Tags.Projects;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.ZLan;

public static class TagsProject_Extensions
{
    public static TagsProjectServiceBuilder AddZLanTcpSupport(this TagsProjectServiceBuilder builder)
    {
        // register channel factory
        builder.Services.AddKeyedSingleton<IChannelFactory, ZLanTcpChannelFactory>(ZLanTcpNames.DriverName);
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var factory = sp.GetRequiredKeyedService<IChannelFactory>(ZLanTcpNames.DriverName);
            composite.AddFactory(factory);
        });

        // register tags loader
        builder.ConfigTagsLoader((sp, composite) => { 
            composite.AddTagsCbntBuilder<ZLanDICbntBuilder>(ZLanTcpNames.DriverName, predicate: b => b.Area == "DI");
            composite.AddTagsCbntBuilder<ZLanDOCbntBuilder>(ZLanTcpNames.DriverName, predicate: b => b.Area == "DO");
        });

        return builder;
    }
}
