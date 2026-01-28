using Itminus.Tags.Projects;
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
        services.AddSingleton<ITagsProjectRunner, TagsProjectRunner>();
        services.AddSingleton<ILogicetLoader, LogicetLoader>();

        var tpsb = new TagsProjectServiceBuilder(services);
        configTagsLoader?.Invoke(tpsb);

        services.AddSingleton<IChannelFactory, CompositeChannelFactory>(sp =>
        {
            tpsb.ApplyChannelFactoriesConfiguration(sp);
            return tpsb.ChannelFactories;
        });
        services.AddSingleton<ITagsLoader>(sp =>
        {
            tpsb.ApplyTagsLoadersConfiguration(sp);
            return tpsb.TagsLoaders;
        });
        return services;
    }
}


public class TagsProjectServiceBuilder
{
    public TagsProjectServiceBuilder(IServiceCollection services) 
    {
        this.Services = services;
        this.ChannelFactories = new CompositeChannelFactory();
        this.ChannelFactoriesConfiguration = new List<Action<IServiceProvider, CompositeChannelFactory>>();

        this.TagsLoaders = new CompositeTagsLoader();
        this.TagsLoadersConfiguration = new List<Action<IServiceProvider, CompositeTagsLoader>>(); 

    }

    public IServiceCollection Services { get; }


    #region ChannelFactories
    public CompositeChannelFactory ChannelFactories { get; set; }
    protected IList<Action<IServiceProvider, CompositeChannelFactory>> ChannelFactoriesConfiguration { get; set; }


    /// <summary>
    /// 配置ChannelFactory
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public TagsProjectServiceBuilder ConfigChannelsFactory(Action<IServiceProvider, CompositeChannelFactory> config)
    {
        this.ChannelFactoriesConfiguration.Add(config);
        return this;
    }

    /// <summary>
    /// 应用 通道工厂 配置
    /// </summary>
    /// <param name="sp"></param>
    internal void ApplyChannelFactoriesConfiguration(IServiceProvider sp)
    {
        foreach(var config in this.ChannelFactoriesConfiguration)
        {
            config.Invoke(sp, this.ChannelFactories);
        }
    }
    #endregion



    #region TagLoaders
    public CompositeTagsLoader TagsLoaders { get; set; }
    protected IList<Action<IServiceProvider, CompositeTagsLoader>> TagsLoadersConfiguration { get; set; }

    /// <summary>
    /// 配置TagsLoader
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public TagsProjectServiceBuilder ConfigTagsLoader(Action<IServiceProvider, CompositeTagsLoader> config)
    {
        this.TagsLoadersConfiguration.Add(config);
        return this;
    }

    /// <summary>
    /// 应用 测点加载器 配置
    /// </summary>
    /// <param name="sp"></param>
    internal void ApplyTagsLoadersConfiguration(IServiceProvider sp)
    {
        foreach (var config in this.TagsLoadersConfiguration)
        {
            config.Invoke(sp, this.TagsLoaders);
        }
    }
    #endregion




}