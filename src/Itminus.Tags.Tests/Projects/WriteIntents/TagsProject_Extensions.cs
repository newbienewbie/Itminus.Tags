using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.Tests.Projects.WriteIntents;

internal static class TagsProject_Extensions
{
    internal static TagsProjectServiceBuilder AddFakedSupport(this TagsProjectServiceBuilder builder)
    {
        builder.Services.AddKeyedSingleton<ITagChannelFactory, FakedChannelFactory>("fake");
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var factory = sp.GetRequiredKeyedService<ITagChannelFactory>("fake");
            composite.AddFactory(factory);
        });

        return builder;
    }
}
