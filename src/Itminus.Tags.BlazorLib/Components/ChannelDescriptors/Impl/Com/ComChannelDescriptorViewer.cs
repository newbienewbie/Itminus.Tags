using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Editing;
using Itminus.Tags.ComScanner;
using Itminus.Tags.ComScanner.Channels;
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.Com;

sealed class ComChannelDescriptorViewer : ITagChannelDescriptorViewer
{
    public int Priority => 100;
    public bool CanView(TagChannelDescriptor descriptor) => descriptor.Driver == ComScannerNames.DriverName;

    public RenderFragment View(TagChannelDescriptor descriptor)
    {
        var d = descriptor.ToComChannelDescriptor();
        return builder =>
        {
            builder.OpenComponent<ComChannelDescriptorViewerView>(0);
            builder.AddAttribute(1, nameof(ComChannelDescriptorViewerView.Descriptor), d);
            builder.CloseComponent();
        };
    }
}
