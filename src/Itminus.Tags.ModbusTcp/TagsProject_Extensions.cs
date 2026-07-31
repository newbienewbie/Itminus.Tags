using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.ModbusTcp;

/// <summary>
/// extensions for DependencyInjection
/// </summary>
public static class TagsProject_Extensions
{
    /// <summary>
    /// 添加 ModbusTcp 支持。是 <see cref="AddModbusTcpChannel"/>、<see cref="AddModbusTcpTagCbntBuilder"/> 与 <see cref="AddModbusTcpTagBuilder"/> 的组合
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddModbusTcpSupport(this TagsProjectServiceBuilder builder)
    {
        builder
            .AddModbusTcpChannel()
            .AddModbusTcpTagCbntBuilder()
            .AddModbusTcpTagBuilder();
        return builder;
    }

#region 基本扩展
    /// <summary>
    /// 注册ModbusTcp支持——仅注册ChannelFactory，不注册TagBuilder/TagCbntBuilder <br/>
    /// 作用是在通道的驱动为 <see cref="ModbusTcpNames.DriverName"/> 时，会尝试构建一个通道。
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddModbusTcpChannel(this TagsProjectServiceBuilder builder)
    {
        // register channel factory
        builder.Services.AddKeyedSingleton<ITagChannelFactory, ModbusTcpChannelFactory>(ModbusTcpNames.DriverName);
        builder.ConfigChannelsFactory((sp, composite) =>
        {
            var factory = sp.GetRequiredKeyedService<ITagChannelFactory>(ModbusTcpNames.DriverName);
            composite.AddFactory(factory);
        });
        return builder;
    }

    /// <summary>
    /// 注册ModbusTcp支持——仅注册测点组合构建器（TagCbntBuilder），不注册ChannelFactory/TagBuilder <br/>
    /// 作用是在通道的驱动为 <see cref="ModbusTcpNames.DriverName"/> 时，会尝试构建一个测点组合。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">配置TagCbntBuilder的回调</param>
    /// <param name="predicate">用于过滤TagCbntBuilder的谓词</param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddModbusTcpTagCbntBuilder(
        this TagsProjectServiceBuilder builder,
        Action<ModbusTcpTagCbntBuilder>? configure = null,
        Func<ModbusTcpTagCbntBuilder, bool>? predicate = null
        )
    {
        // register cbnt loader
        builder.ConfigTagsLoader((sp, composite) =>
        {
            composite.AddTagsCbntBuilder<ModbusTcpTagCbntBuilder>(ModbusTcpNames.DriverName, configure, predicate);
        });
        return builder;
    }

    /// <summary>
    /// 注册ModbusTcp支持——仅注册直接测点构建器（TagBuilder），不注册ChannelFactory/TagCbntBuilder <br/>
    /// 作用是在通道的驱动为 <see cref="ModbusTcpNames.DriverName"/> 时，会尝试构建一个测点。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">配置TagBuilder的回调</param>
    /// <param name="predicate">用于过滤TagBuilder的谓词</param>
    /// <returns></returns>
    public static TagsProjectServiceBuilder AddModbusTcpTagBuilder(
        this TagsProjectServiceBuilder builder,
        Action<ModbusTcpDirectTagBuilder>? configure = null,
        Func<ModbusTcpDirectTagBuilder, bool>? predicate = null
        )
    {
        // register tags loader
        builder.ConfigTagsLoader((sp, composite) =>
        {
            composite.AddTagBuilder<ModbusTcpDirectTagBuilder>(ModbusTcpNames.DriverName, configure, predicate);
        });
        return builder;
    }
#endregion
}
