using Itminus.Tags.Projects;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.ModbusTcp;

public static class TagsProject_Extensions
{
    public static TagsProjectServiceBuilder AddModbusTcpSupport(this TagsProjectServiceBuilder builder)
    {
        // register channel factory
        builder.Services.AddKeyedSingleton<IChannelFactory, ModbusTcpChannelFactory>(ModbusTcpNames.DriverName);
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var factory = sp.GetRequiredKeyedService<IChannelFactory>(ModbusTcpNames.DriverName);
            composite.AddFactory(factory);
        });

        // register tags loader
        builder.ConfigTagsLoader((sp, composite) => { 
            composite.AddTagsCbntBuilder<ModbusTcpTagCbntBuilder>(ModbusTcpNames.DriverName);
        });

        return builder;
    }
}
