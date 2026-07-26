using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.SimpleFiles;

/// <summary>
/// extensions for <see cref="TagsProjectServiceBuilder"/> to add SimpleFiles support
/// </summary>
public static class TagsProject_Extensions
{
    /// <summary>
    /// 注册SimpleFiles支持
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddSimpleFilesSupport(this TagsProjectServiceBuilder builder)
    {
        // register SimpleFiles channel factory
        builder.Services.AddKeyedSingleton<ITagChannelFactory, SimpleFilesTagChannelFactory>(SimpleFilesNames.DriverName);
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var simpleFilesFactory = sp.GetRequiredKeyedService<ITagChannelFactory>(SimpleFilesNames.DriverName);
            composite.AddFactory(simpleFilesFactory);
        });

        // register SimpleFiles tags loader
        builder.ConfigTagsLoader((sp, composite) => { 
            composite.AddTagBuilder<SimpleTagsDirectTagBuilder>(SimpleFilesNames.DriverName);
        });

        return builder;
    }
}
