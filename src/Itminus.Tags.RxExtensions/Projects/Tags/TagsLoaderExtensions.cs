using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.Projects;

public static class TagsLoaderExtensions
{
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