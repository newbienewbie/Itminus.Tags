using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags;

public static class TagsProjectServiceCollection
{

    /// <summary>
    /// 注册测点项目服务，其中可以配置通道工厂、组合测点加载器
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configTagsLoader"></param>
    /// <returns></returns>
    public static IServiceCollection AddTagsProjectServices(this IServiceCollection services,  Action<TagsProjectServiceBuilder> configTagsLoader)
    {
        var tpsb = new TagsProjectServiceBuilder(services);
        configTagsLoader?.Invoke(tpsb);

        services.AddSingleton<ITagChannelFactory>(sp =>
        {
            tpsb.ApplyChannelFactoriesConfiguration(sp);
            return tpsb.ChannelFactories;
        });
        services.AddSingleton<ITagsLoader>(sp =>
        {
            tpsb.ApplyTagsLoadersConfiguration(sp);
            return tpsb.TagsLoaders;
        });

        services.AddScoped<ITagsProjectFactory, TagsProjectFactory>();

        return services;
    }
}
