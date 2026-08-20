using Itminus.Tags.Core.Projects;
using Itminus.Tags.Logicets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itminus.Tags;

/// <summary>
///  builder for configuring and building services related to tags projects,
///  including 
///     channel factories, 
///     tag loaders, 
///     and logicet load options.
/// </summary>
public class TagsProjectServiceBuilder
{
    /// <summary>
    /// c'tor
    /// </summary>
    public TagsProjectServiceBuilder(IServiceCollection services) 
    {

        this.Services = services;

        this.ChannelFactories = new CompositeTagChannelFactory();
        this.ChannelFactoriesConfiguration = new List<Action<IServiceProvider, CompositeTagChannelFactory>>();

        this.TagsLoaders = new CompositeTagsLoader();
        this.TagsLoadersConfiguration = new List<Action<IServiceProvider, CompositeTagsLoader>>(); 

        this.LogicetLoadOptionsBuilder = this.Services.AddOptions<LogicetLoadOptions>(); 
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


    #region
    /// <summary>
    /// 业务逻辑选项的构建器
    /// </summary>
    public OptionsBuilder<LogicetLoadOptions> LogicetLoadOptionsBuilder { get; }
    #endregion

    #region Schema 校验
    /// <summary>
    /// 启用加载期 XSD 校验（可选功能）。<br/>
    /// 启用后，<see cref="ITagsProjectFactory.Create"/> / <c>MakeProject</c> 在加载通道与测点之前，
    /// 会用嵌入程序集的 XSD（<see cref="TagsProjectSchema"/>）校验项目 XML；
    /// 不通过时抛出 <see cref="TagsProjectSchemaException"/>。<br/>
    /// 默认关闭——老的“无命名空间前缀”XML 配置不启用校验时照常工作。
    /// </summary>
    /// <returns></returns>
    public TagsProjectServiceBuilder EnableXmlSchemaValidation()
    {
        this.Services.AddSingleton<ITagsProjectSchemaValidator, TagsProjectSchemaValidator>();
        return this;
    }
    #endregion


    private TagsProjectServiceBuilder AddDefaults()
    {
        this.Services.AddSingleton<ITagGrpRunnerFactory, TagGrpRunnerFactory>();
        this.Services.AddSingleton<ILogicetsLoader, LogicetLoader>();
        this.Services.AddScoped<ITagsProjectFactory, TagsProjectFactory>();
        return this;
    }

    /// <summary>
    /// 构建服务，注册必要的服务和配置。<br/>
    /// </summary>
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

/// <summary>
/// 业务逻辑加载选项，用于配置 Logicet 插件的加载行为。<br/>
/// </summary>
public class LogicetLoadOptions
{
    /// <summary>
    /// 共享类型过滤器
    /// </summary>
    public LogicetSharedTypesFilter? SharedTypesFilter { get; set;} 
}

/// <summary>
/// 共享类型过滤器，用于在加载 Logicet 插件时指定哪些类型需要在主程序和插件之间共享。
/// 通过实现这个委托，用户可以动态地添加或修改共享类型列表
/// </summary>
/// <param name="dll">dll 文件路径</param>
/// <param name="sharedTypes">共享类型列表</param>
public delegate void LogicetSharedTypesFilter(string dll, List<Type> sharedTypes);