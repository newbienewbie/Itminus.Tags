using Itminus.Tags.ComScanner.Channels;
using Itminus.Tags.ComScanner.Tags;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.ComScanner;

/// <summary>
/// extensions for TagsProjectServiceBuilder to add COM scanner support
/// </summary>
public static class TagsProject_Extensions
{
    /// <summary>
    /// 注册COM支持。是 <see cref="AddComScannerChannel"/> 与 <see cref="AddComScannerTagBuilder"/> 的组合
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddComScannerSupport(this TagsProjectServiceBuilder builder)
    {
        builder
            .AddComScannerChannel()
            .AddComScannerTagBuilder();
        return builder;
    }

#region 基本扩展
    /// <summary>
    /// 注册COM 支持——仅注册ChannelFactory，不注册TagBuilder <br/>
    /// 作用是在通道的驱动为 <see cref="ComDriverNames.DriverName"/> 时，会尝试构建一个通道。
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddComScannerChannel(this TagsProjectServiceBuilder builder)
    {
        // register channel factory
        builder.Services.AddKeyedSingleton<ITagChannelFactory, ComChannelFactory>(ComDriverNames.DriverName);
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var factory = sp.GetRequiredKeyedService<ITagChannelFactory>(ComDriverNames.DriverName);
            composite.AddFactory(factory);
        });
        return builder;
    }

    /// <summary>
    /// 注册COM 支持——仅注册TagBuilder，不注册ChannelFactory。<br/>
    /// 作用是在通道的驱动为 <see cref="ComDriverNames.DriverName"/> 时，会尝试构建一个测点；<br/>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">配置TagBuilder的回调</param>
    /// <param name="predicate">用于过滤TagBuilder的谓词</param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddComScannerTagBuilder(
        this TagsProjectServiceBuilder builder,
        Action<ComTagBuilder>? configure = null,
        Func<ComTagBuilder, bool>? predicate = null
        )
    {
        // register tags loader
        builder.ConfigTagsLoader((sp, composite) => { 
            composite.AddTagBuilder<ComTagBuilder>(ComDriverNames.DriverName, configure, predicate);
        });

        return builder;
    }
#endregion
}
