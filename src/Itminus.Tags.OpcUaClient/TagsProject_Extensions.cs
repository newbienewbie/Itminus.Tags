using Itminus.Tags.OpcUaClient.Cbnts;
using Itminus.Tags.OpcUaClient.DirectTags;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.OpcUaClient;

/// <summary>
/// extensions for DependencyInjection
/// </summary>
public static class TagsProject_Extensions
{
    /// <summary>
    /// 添加 OpcUaClient 支持。是 <see cref="AddOpcUaClientChannel"/>、<see cref="AddOpcUaClientTagCbntBuilder"/> 与 <see cref="AddOpcUaClientTagBuilder"/> 的组合
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddOpcUaClientSupport(this TagsProjectServiceBuilder builder)
    {
        builder
            .AddOpcUaClientChannel()
            .AddOpcUaClientTagCbntBuilder()
            .AddOpcUaClientTagBuilder();
        return builder;
    }

#region 基本扩展
    /// <summary>
    /// 注册OpcUaClient支持——仅注册ChannelFactory，不注册TagBuilder/TagCbntBuilder <br/>
    /// 作用是在通道的驱动为 <see cref="OpcUaClientNames.DriverName"/> 时，会尝试构建一个通道。
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddOpcUaClientChannel(this TagsProjectServiceBuilder builder)
    {
        builder.Services.AddKeyedSingleton<ITagChannelFactory, OpcUaClientTagChannelFactory>(OpcUaClientNames.DriverName);
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var factory = sp.GetRequiredKeyedService<ITagChannelFactory>(OpcUaClientNames.DriverName);
            composite.AddFactory(factory);
        });
        return builder;
    }

    /// <summary>
    /// 注册OpcUaClient支持——仅注册测点组合构建器（TagCbntBuilder），不注册ChannelFactory/TagBuilder <br/>
    /// 作用是在通道的驱动为 <see cref="OpcUaClientNames.DriverName"/> 时，会尝试构建一个测点组合。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">配置TagCbntBuilder的回调</param>
    /// <param name="predicate">用于过滤TagCbntBuilder的谓词</param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddOpcUaClientTagCbntBuilder(
        this TagsProjectServiceBuilder builder,
        Action<OpcUaClientTagCbntBuilder>? configure = null,
        Func<OpcUaClientTagCbntBuilder, bool>? predicate = null
        )
    {
        builder.ConfigTagsLoader((sp, composite) =>
        {
            composite.AddTagsCbntBuilder<OpcUaClientTagCbntBuilder>(OpcUaClientNames.DriverName, configure, predicate);
        });
        return builder;
    }

    /// <summary>
    /// 注册OpcUaClient支持——仅注册直接测点构建器（TagBuilder），不注册ChannelFactory/TagCbntBuilder <br/>
    /// 作用是在通道的驱动为 <see cref="OpcUaClientNames.DriverName"/> 时，会尝试构建一个测点。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">配置TagBuilder的回调</param>
    /// <param name="predicate">用于过滤TagBuilder的谓词</param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddOpcUaClientTagBuilder(
        this TagsProjectServiceBuilder builder,
        Action<OpcUaClientTagBuilder>? configure = null,
        Func<OpcUaClientTagBuilder, bool>? predicate = null
        )
    {
        builder.ConfigTagsLoader((sp, composite) =>
        {
            composite.AddTagBuilder<OpcUaClientTagBuilder>(OpcUaClientNames.DriverName, configure, predicate);
        });
        return builder;
    }
#endregion
}
