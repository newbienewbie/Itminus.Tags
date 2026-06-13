using Microsoft.Extensions.DependencyInjection;

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
            composite.AddTagBuilder<S7DirectTagBuilder>(S7Names.DriverName);
        });

        return builder;
    }
}
