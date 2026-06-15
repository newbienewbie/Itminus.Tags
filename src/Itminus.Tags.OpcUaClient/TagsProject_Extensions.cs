using Itminus.Tags.OpcUaClient.Cbnts;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.OpcUaClient;

public static class TagsProject_Extensions
{
    public static TagsProjectServiceBuilder AddOpcUaClientSupport(this TagsProjectServiceBuilder builder)
    {
        builder.Services.AddKeyedSingleton<ITagChannelFactory, OpcUaClientTagChannelFactory>(OpcUaClientNames.DriverName);
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var factory = sp.GetRequiredKeyedService<ITagChannelFactory>(OpcUaClientNames.DriverName);
            composite.AddFactory(factory);
        });

        builder.ConfigTagsLoader((sp, composite)=> {
            composite.AddTagsCbntBuilder<OpcUaClientTagCbntBuilder>(OpcUaClientNames.DriverName);
        });

        return builder;
    }
}
