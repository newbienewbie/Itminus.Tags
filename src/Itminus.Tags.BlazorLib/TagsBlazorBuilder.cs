using Itminus.Tags.BlazorLib.Components.Channels.Editing;

using Itminus.Tags.BlazorLib.Components.Tags.Editing;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.BlazorLib;

public class TagsBlazorBuilder
{
    public IServiceCollection Services { get; }
    public TagsBlazorBuilder(IServiceCollection services)
    {
        services.AddScoped<TagsBlazorLibJsInterop>();
        // Registry
        services.AddScoped<TagValueEditorRegistry>();
        services.AddScoped<TagChannelDescriptorRendererRegistry>();

        Services = services;
    }

    public TagsBlazorBuilder AddTagValueEditor<TEditor>()
        where TEditor : class, ITagValueEditor
    {
        this.Services.AddScoped<ITagValueEditor, TEditor>();
        return this;
    }

    public TagsBlazorBuilder AddTagChannelDescriptorViewer<TViewer>()
        where TViewer : class, ITagChannelDescriptorViewer
    {
        this.Services.AddScoped<ITagChannelDescriptorViewer, TViewer>();
        return this;
    }

    public TagsBlazorBuilder AddTagChannelDescriptorEditor<TEditor>()
        where TEditor : class, ITagChannelDescriptorEditor
    {
        this.Services.AddScoped<ITagChannelDescriptorEditor, TEditor>();
        return this;
    }


    public void Build()
    {
        return;
    }
}
