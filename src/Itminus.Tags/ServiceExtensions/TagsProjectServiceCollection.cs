using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Xml.Linq;

namespace Itminus.Tags;

/// <summary>
/// extensions for DependencyInjection
/// </summary>
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
        tpsb.Build();

        services.AddSingleton<ITagsProjectCtrl, TagsProjectCtrl>();
        return services;
    }


    /// <summary>
    /// 以指定的key 注册 <see cref="ITagsProjectCtrl" />
    /// </summary>
    /// <param name="services"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static IServiceCollection AddKeyedTagsProjectCtrl(this IServiceCollection services,  string key)
    {
        services.AddKeyedSingleton<ITagsProjectCtrl, TagsProjectCtrl>(key);
        return services;
    }

    /// <summary>
    /// 构建测点项目实例
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="dir">如果为空，则使用当前程序集所在目录，如果仍为空，则使用应用程序数据目录</param>
    /// <param name="root">根元素，如果为空，则使用使用index.xml构建</param>
    /// <returns></returns>
    public static ITagsProject MakeProject(this IServiceProvider sp, string? dir = null, XElement? root = null)
    {
        var factory = sp.GetRequiredService<ITagsProjectFactory>();

        if (string.IsNullOrEmpty(dir))
        {
            var loc = Assembly.GetExecutingAssembly().Location;
            dir = Path.GetDirectoryName(loc);
        }
        if (string.IsNullOrEmpty(dir))
        {
            dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        var proj = factory.Create(dir!, root);
        return proj;
    }

}
