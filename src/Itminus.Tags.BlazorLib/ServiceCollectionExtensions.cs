using Itminus.Tags.BlazorLib.Components.Tags.Editing;
using Itminus.Tags.BlazorLib.Components.Tags.Editing.DefaultEditors;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.BlazorLib;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTagsBlazorLib(this IServiceCollection services)
    {
        // Registry
        services.AddScoped<TagValueEditorRegistry>();

        services.AddTagValueEditor<BoolTagValueEditor>()
                .AddTagValueEditor<NumericTagValueEditor>()
                .AddTagValueEditor<TextTagValueEditor>();

        return services;
    }


    public static IServiceCollection AddTagValueEditor<TTagValueEditor>(this IServiceCollection services)
        where TTagValueEditor:class, ITagValueEditor
    {
        services.AddScoped<ITagValueEditor, TTagValueEditor>();
        return services;
    }
}
