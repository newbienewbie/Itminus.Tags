using Itminus.Tags.ComScanner.Channels;
using Itminus.Tags.ComScanner.Tags;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.ComScanner;

public static class TagsProject_Extensions
{
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
