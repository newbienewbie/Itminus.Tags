using Microsoft.Extensions.DependencyInjection;
using System.Xml.Linq;

namespace Itminus.Tags;

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
    /// 注册特定驱动的 TagsCbnt 加载器: 
    ///     如果将来被送入加载器的Tag的channel与这里指定的驱动相同，则会尝试构建一个测点组合；<br/>
    ///     如果配置了predicate且 predicate调用后给出true，则还会再尝试一次过滤<br/>
    /// </summary>
    /// <typeparam name="TCbntBuilder"></typeparam>
    /// <param name="loader"></param>
    /// <param name="sp"></param>
    /// <param name="driver"></param>
    /// <returns></returns>
    public static CompositeTagsLoader AddTagsCbntBuilder<TCbntBuilder>(this CompositeTagsLoader loader, string driver, Func<TCbntBuilder, bool>? predicate = null)
        where TCbntBuilder : TagCbntBuilderBase, new()
    {
        return loader.AddTagsCbntBuilder((channel, el) => {
            if (channel.Driver != driver)
            {
                return null;
            }
            var builder = new TCbntBuilder();
            builder.WithXElement(el);

            var flag = predicate is null ? true : predicate(builder);
            if (!flag)
            {
                return null;
            }
            return builder;
        });
    }
}