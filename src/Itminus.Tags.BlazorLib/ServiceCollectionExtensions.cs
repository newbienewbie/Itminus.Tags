using Itminus.Tags.BlazorLib.Components.Channels.Impl.OpcUa;
using Itminus.Tags.BlazorLib.Components.Channels.Impl.ZLan;
using Itminus.Tags.BlazorLib.Components.Channels.Editing;
using Itminus.Tags.BlazorLib.Components.Channels.Impl.ComScanner;
using Itminus.Tags.BlazorLib.Components.Channels.Impl.Hjzk;
using Itminus.Tags.BlazorLib.Components.Channels.Impl.ModbusTcp;
using Itminus.Tags.BlazorLib.Components.Channels.Impl.S7;

using Itminus.Tags.BlazorLib.Components.Tags.Editing;
using Itminus.Tags.BlazorLib.Components.Tags.Editors;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.BlazorLib;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTagsBlazorLib(this IServiceCollection services)
    {
        services.AddScoped<TagsBlazorLibJsInterop>();
        // Registry
        services.AddScoped<TagValueEditorRegistry>();
        services.AddScoped<TagChannelDescriptorRendererRegistry>();

        services.AddTagValueEditor<BoolTagValueEditor>()
                .AddTagValueEditor<NumericTagValueEditor>()
                .AddTagValueEditor<TextTagValueEditor>();

        // Default channel descriptor viewers/editors
        services.AddTagChannelDescriptorViewer<S7TagChannelDescriptorViewer>()
                .AddTagChannelDescriptorEditor<S7TagChannelDescriptorEditor>()
                .AddTagChannelDescriptorViewer<ModbusTcpTagChannelDescriptorViewer>()
                .AddTagChannelDescriptorEditor<ModbusTcpTagChannelDescriptorEditor>()
                .AddTagChannelDescriptorViewer<HjzkTagChannelDescriptorViewer>()
                .AddTagChannelDescriptorEditor<HjzkTagChannelDescriptorEditor>()
                .AddTagChannelDescriptorViewer<ComScannerTagChannelDescriptorViewer>()
                .AddTagChannelDescriptorEditor<ComScannerTagChannelDescriptorEditor>()
                .AddTagChannelDescriptorViewer<ZLanTcpTagChannelDescriptorViewer>()
                .AddTagChannelDescriptorEditor<ZLanTcpTagChannelDescriptorEditor>()
                .AddTagChannelDescriptorViewer<OpcUaClientTagChannelDescriptorViewer>()
                .AddTagChannelDescriptorEditor<OpcUaClientTagChannelDescriptorEditor>();

        return services;
    }


    public static IServiceCollection AddTagValueEditor<TTagValueEditor>(this IServiceCollection services)
        where TTagValueEditor:class, ITagValueEditor
    {
        services.AddScoped<ITagValueEditor, TTagValueEditor>();
        return services;
    }

    public static IServiceCollection AddTagChannelDescriptorViewer<TViewer>(this IServiceCollection services)
        where TViewer : class, ITagChannelDescriptorViewer
    {
        services.AddScoped<ITagChannelDescriptorViewer, TViewer>();
        return services;
    }

    public static IServiceCollection AddTagChannelDescriptorEditor<TEditor>(this IServiceCollection services)
        where TEditor : class, ITagChannelDescriptorEditor
    {
        services.AddScoped<ITagChannelDescriptorEditor, TEditor>();
        return services;
    }
}
