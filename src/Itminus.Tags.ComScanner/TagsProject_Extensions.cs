using Itminus.Tags.ComScanner.Channels;
using Itminus.Tags.ComScanner.Tags;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.ComScanner;

/// <summary>
/// extensions for TagsProjectServiceBuilder to add COM scanner support
/// </summary>
public static class TagsProject_Extensions
{
    /// <summary>
    /// 注册COM支持
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddComScannerSupport(this TagsProjectServiceBuilder builder)
    {
        // register channel factory
        builder.Services.AddKeyedSingleton<ITagChannelFactory, ComChannelFactory>(ComDriverNames.DriverName);
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var factory = sp.GetRequiredKeyedService<ITagChannelFactory>(ComDriverNames.DriverName);
            composite.AddFactory(factory);
        });

        // register tags loader
        builder.ConfigTagsLoader((sp, composite) => { 
            composite.AddTagBuilder<ComTagBuilder>(ComDriverNames.DriverName);
        });

        return builder;
    }
}
