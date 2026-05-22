using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.OpcUa;
using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.ZLan;
using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.ComScanner;
using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.Hjzk;
using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.ModbusTcp;
using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.S7;
using Itminus.Tags.BlazorLib.Components.Tags.Editors;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Tags.BlazorLib;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTagsBlazorLib(this IServiceCollection services, Action<TagsBlazorBuilder>? config = null)
    {
        var builder = new TagsBlazorBuilder(services);
        // default tag value editors
        builder.AddTagValueEditor<BoolTagValueEditor>()
               .AddTagValueEditor<NumericTagValueEditor>()
               .AddTagValueEditor<TextTagValueEditor>();

        // Default channel descriptor viewers/editors
        builder.AddS7ChannelDescriptorViewerAndEditor()
               .AddModbusTcpChannelDescriptorViewerAndEditor()
               .AddHjzkChannelDescriptorViewerAndEditor()
               .AddComScannerChannelDescriptorViewerAndEditor()
               .AddZLanChannelDescriptorViewerAndEditor()
               .AddOpcUaChannelDescriptorViewerAndEditor();

        // user configuration
        config?.Invoke(builder);
        builder.Build();
        return builder.Services;
    }



}
