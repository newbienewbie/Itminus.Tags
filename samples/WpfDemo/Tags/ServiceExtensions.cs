using Itminus.Tags;
using Itminus.Tags.S7;
using Microsoft.Extensions.DependencyInjection;

namespace WpfDemo.Tags;

internal static class ServiceExtensions
{
    public static IServiceCollection AddWpfDemoTags(this IServiceCollection services)
    {
        services.AddTagsProjectServices(builder =>
        {
            builder.AddS7Support();
        });
        services.AddSingleton<TagsProjectCtrl>();
        return services;
    }

}
