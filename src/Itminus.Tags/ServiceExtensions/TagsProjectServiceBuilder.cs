using Itminus.Tags.Core.Projects;
using Itminus.Tags.Logicets;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags;

public class TagsProjectServiceBuilder
{
    public TagsProjectServiceBuilder(IServiceCollection services) 
    {

        this.Services = services;

        this.ChannelFactories = new CompositeTagChannelFactory();
        this.ChannelFactoriesConfiguration = new List<Action<IServiceProvider, CompositeTagChannelFactory>>();

        this.TagsLoaders = new CompositeTagsLoader();
        this.TagsLoadersConfiguration = new List<Action<IServiceProvider, CompositeTagsLoader>>(); 
    }

    /// <summary>
    /// 服务
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// 使用默认加载器和工厂
    /// </summary>
    public bool UseDefaults { get; set; } = true;

    #region ChannelFactories
    /// <summary>
    /// 通道工厂
    /// </summary>
    public CompositeTagChannelFactory ChannelFactories { get; set; }

    /// <summary>
    /// 通道工厂配置，用于配置<see cref="ChannelFactories"/>
    /// </summary>
    protected IList<Action<IServiceProvider, CompositeTagChannelFactory>> ChannelFactoriesConfiguration { get; set; }


    /// <summary>
    /// 配置ChannelFactory
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public TagsProjectServiceBuilder ConfigChannelsFactory(Action<IServiceProvider, CompositeTagChannelFactory> config)
    {
        this.ChannelFactoriesConfiguration.Add(config);
        return this;
    }

    /// <summary>
    /// 应用 <see cref="ChannelFactoriesConfiguration"/> 里的配置
    /// </summary>
    /// <param name="sp"></param>
    private void ApplyChannelFactoriesConfiguration(IServiceProvider sp)
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
    private void ApplyTagsLoadersConfiguration(IServiceProvider sp)
    {
        foreach (var config in this.TagsLoadersConfiguration)
        {
            config.Invoke(sp, this.TagsLoaders);
        }
    }
    #endregion


    private TagsProjectServiceBuilder AddDefaults()
    {
        this.Services.AddSingleton<ITagGrpRunnerFactory, TagGrpRunnerFactory>();
        this.Services.AddSingleton<ILogicetsLoader, LogicetLoader>();
        this.Services.AddScoped<ITagsProjectFactory, TagsProjectFactory>();
        return this;
    }


    public void Build()
    {
        // channels/tags
        this.Services.AddSingleton<ITagChannelsLoader, TagChannelsLoader>();
        this.Services.AddSingleton<ITagChannelFactory>(sp =>
        {
            this.ApplyChannelFactoriesConfiguration(sp);
            return this.ChannelFactories;
        });
        this.Services.AddSingleton<ITagsLoader>(sp =>
        {
            this.ApplyTagsLoadersConfiguration(sp);
            return this.TagsLoaders;
        });

        
        // defaults
        if (this.UseDefaults)
        {
            this.AddDefaults();
        }
    }


}