using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.S7;

public static class TagsProject_Extensions
{
    public static TagsProjectServiceBuilder AddS7Support(this TagsProjectServiceBuilder builder)
    {
        // register S7 channel factory
        builder.Services.AddKeyedSingleton<ITagChannelFactory, S7TagChannelFactory>(S7Names.DriverName);
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var s7Factory = sp.GetRequiredKeyedService<ITagChannelFactory>(S7Names.DriverName);
            composite.AddFactory(s7Factory);
        });

        // register S7 tags loader
        builder.ConfigTagsLoader((sp, composite) => { 
            composite.AddTagsCbntBuilder<S7TagCbntBuilder>(S7Names.DriverName);
        });

        return builder;
    }
}
