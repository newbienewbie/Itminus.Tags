using Itminus.Tags.BlazorLib.Components.Channels.Editing;
using Itminus.Tags.ComScanner;
using Itminus.Tags.ComScanner.Channels;
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.Channels.Impl.ComScanner;

sealed class ComScannerTagChannelDescriptorEditor : ITagChannelDescriptorEditor
{
    public int Priority => 100;
    public bool CanEdit(TagChannelDescriptor descriptor) => descriptor.Driver == ComScannerNames.DriverName;

    public RenderFragment Edit(TagChannelDescriptor descriptor, Action<TagChannelDescriptor> descriptorChanged)
    {
        var d = descriptor.ToComScannerTagChannelDescriptor();
        if (!ReferenceEquals(d, descriptor))
        {
            descriptorChanged(d);
        }

        return builder =>
        {
            builder.OpenComponent<ComScannerTagChannelDescriptorEditorView>(0);
            builder.AddAttribute(1, nameof(ComScannerTagChannelDescriptorEditorView.Descriptor), d);
            builder.CloseComponent();
        };
    }
}
