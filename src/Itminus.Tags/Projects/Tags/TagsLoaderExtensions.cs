using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.Projects;

public static class TagsLoaderExtensions
{
    //public static CompositeTagsLoader AddTagBuilder<TTagBuilder>(this CompositeTagsLoader loader, IServiceProvider sp, string driver)
    //    where TTagBuilder : TagBuilderBase, new()
    //{
    //    return loader.AddTagBuilder((channel, descriptor, el) => {
    //        if (channel.Driver != driver)
    //        {
    //            return null;
    //        }
    //        var name = el.GetTagUnionName();
    //        var addr = el.GetTagUnionAddress(name);
    //        var builder = ActivatorUtilities.CreateInstance<TTagBuilder>(sp, name, addr);
    //        return builder.WithDevice(channel);
    //    });
    //}

    /// <summary>
    /// 注册一个针对测点组合的加载器: 
    ///     如果将来被送入加载器的Tag的channel与这里指定的驱动相同，则会构建一个测点组合<br/>
    /// </summary>
    /// <typeparam name="TCbntBuilder"></typeparam>
    /// <param name="loader"></param>
    /// <param name="sp"></param>
    /// <param name="driver"></param>
    /// <returns></returns>
    public static CompositeTagsLoader AddTagsCbntBuilder<TCbntBuilder>(this CompositeTagsLoader loader, IServiceProvider sp, string driver)
        where TCbntBuilder: TagCbntBuilderBase, new()
    {
        return loader.AddTagsCbntBuilder((channel, el) => {
            if (channel.Driver != driver)
            {
                return null;
            }
            var name = el.GetTagUnionName();
            var addr = el.GetTagUnionAddress(name);
            var builder = ActivatorUtilities.CreateInstance<TCbntBuilder>(sp);
            builder.SetNameAndAddress(name, addr);
            return builder.WithChannel(channel);
        });
    }
}