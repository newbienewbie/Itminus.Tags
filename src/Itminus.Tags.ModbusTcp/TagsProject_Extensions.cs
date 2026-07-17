using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.ModbusTcp;

/// <summary>
/// extensions for DependencyInjection
/// </summary>
public static class TagsProject_Extensions
{
    /// <summary>
    /// 添加 ModbusTcp 支持
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddModbusTcpSupport(this TagsProjectServiceBuilder builder)
    {
        // register channel factory
        builder.Services.AddKeyedSingleton<ITagChannelFactory, ModbusTcpChannelFactory>(ModbusTcpNames.DriverName);
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var factory = sp.GetRequiredKeyedService<ITagChannelFactory>(ModbusTcpNames.DriverName);
            composite.AddFactory(factory);
        });

        // register tags loader
        builder.ConfigTagsLoader((sp, composite) => { 
            composite.AddTagsCbntBuilder<ModbusTcpTagCbntBuilder>(ModbusTcpNames.DriverName);
            composite.AddTagBuilder<ModbusTcpDirectTagBuilder>(ModbusTcpNames.DriverName);
        });

        return builder;
    }
}
