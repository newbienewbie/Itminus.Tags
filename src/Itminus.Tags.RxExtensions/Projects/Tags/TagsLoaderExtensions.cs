using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.Projects;

public static class TagsLoaderExtensions
{
    /// <summary>
    /// 向 <see cref="CompositeTagsLoader"/> 中添加一个针对测点组合的加载器，在条件满足的情况下，会构建一个测点组合<br/>
    /// </summary>
    /// <typeparam name="TCbntBuilder"></typeparam>
    /// <param name="loader"></param>
    /// <param name="sp"></param>
    /// <param name="driver"></param>
    /// <returns></returns>
    public static CompositeTagsLoader AddTagsCbntBuilder<TCbntBuilder>(this CompositeTagsLoader loader, IServiceProvider sp, string driver)
        where TCbntBuilder: TagCbntBuilderBase
    {
        return loader.AddTagsCbntBuilder((channel, el) => {
            if (channel.Driver != driver)
            {
                return null;
            }
            var name = el.GetTagUnionName();
            var addr = el.GetTagUnionAddress(name);
            var builder = ActivatorUtilities.CreateInstance<TCbntBuilder>(sp, name, addr);
            return builder.WithDevice(channel);
        });
    }
}