using Itminus.Tags.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags;

public class TagsProjectServiceBuilder
{
    public TagsProjectServiceBuilder(IServiceCollection services) 
    {
        services.AddSingleton<ILogicetLoader, LogicetLoader>();
        services.AddSingleton<IChannelsLoader, ChannelsLoader>();

        this.Services = services;
        this.ChannelFactories = new CompositeChannelFactory();
        this.ChannelFactoriesConfiguration = new List<Action<IServiceProvider, CompositeChannelFactory>>();

        this.TagsLoaders = new CompositeTagsLoader();
        this.TagsLoadersConfiguration = new List<Action<IServiceProvider, CompositeTagsLoader>>(); 

    }

    /// <summary>
    /// 服务
    /// </summary>
    public IServiceCollection Services { get; }


    #region ChannelFactories
    /// <summary>
    /// 通道工厂
    /// </summary>
    public CompositeChannelFactory ChannelFactories { get; set; }

    /// <summary>
    /// 通道工厂配置，用于配置<see cref="ChannelFactories"/>
    /// </summary>
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
    /// 应用 <see cref="ChannelFactoriesConfiguration"/> 里的配置
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
    /// <summary>
    /// 通道加载器
    /// </summary>
    public CompositeTagsLoader TagsLoaders { get; set; }

    /// <summary>
    /// 通道加载器的配置
    /// </summary>
    protected IList<Action<IServiceProvider, CompositeTagsLoader>> TagsLoadersConfiguration { get; set; }

    /// <summary>
    /// 配置 <see cref="TagsLoaders"/>
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public TagsProjectServiceBuilder ConfigTagsLoader(Action<IServiceProvider, CompositeTagsLoader> config)
    {
        this.TagsLoadersConfiguration.Add(config);
        return this;
    }

    /// <summary>
    /// 应用 <see cref="TagsLoadersConfiguration"/> 中的配置
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