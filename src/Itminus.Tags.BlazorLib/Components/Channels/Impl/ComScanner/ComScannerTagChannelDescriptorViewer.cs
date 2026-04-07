using Itminus.Tags.BlazorLib.Components.Channels.Editing;
using Itminus.Tags.ComScanner;
using Itminus.Tags.ComScanner.Channels;
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.Channels.Impl.ComScanner;

sealed class ComScannerTagChannelDescriptorViewer : ITagChannelDescriptorViewer
{
    public int Priority => 100;
    public bool CanView(TagChannelDescriptor descriptor) => descriptor.Driver == ComScannerNames.DriverName;

    public RenderFragment View(TagChannelDescriptor descriptor)
    {
        var d = descriptor.ToComScannerTagChannelDescriptor();
        return builder =>
        {
            builder.OpenComponent<ComScannerTagChannelDescriptorViewerView>(0);
            builder.AddAttribute(1, nameof(ComScannerTagChannelDescriptorViewerView.Descriptor), d);
            builder.CloseComponent();
        };
    }
}
