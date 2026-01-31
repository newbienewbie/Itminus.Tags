using Microsoft.Extensions.DependencyInjection;
using Itminus.Tags.Projects;

namespace Itminus.Tags.OpcUaClient;

public static class TagsProject_Extensions
{
    public static TagsProjectServiceBuilder AddOpcUaClientSupport(this TagsProjectServiceBuilder builder)
    {
        builder.Services.AddKeyedSingleton<IChannelFactory, OpcUaClientTagChannelFactory>(OpcUaClientNames.DriverName);
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var factory = sp.GetRequiredKeyedService<IChannelFactory>(OpcUaClientNames.DriverName);
            composite.AddFactory(factory);
        });

        builder.ConfigTagsLoader((sp, composite)=> {
            composite.AddTagsCbntBuilder<OpcUaClientTagCbntBuilder>(OpcUaClientNames.DriverName);
        });

        return builder;
    }
}
