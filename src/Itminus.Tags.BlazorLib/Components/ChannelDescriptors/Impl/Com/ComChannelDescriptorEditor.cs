using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Editing;
using Itminus.Tags.ComScanner;
using Itminus.Tags.ComScanner.Channels;
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.Com;

sealed class ComChannelDescriptorEditor : ITagChannelDescriptorEditor
{
    public int Priority => 100;
    public bool CanEdit(TagChannelDescriptor descriptor) => descriptor.Driver == ComDriverNames.DriverName;

    public RenderFragment Edit(TagChannelDescriptor descriptor, Action<TagChannelDescriptor> descriptorChanged)
    {
        var d = descriptor.ToComChannelDescriptor();
        if (!ReferenceEquals(d, descriptor))
        {
            descriptorChanged(d);
        }

        return builder =>
        {
            builder.OpenComponent<ComChannelDescriptorEditorView>(0);
            builder.AddAttribute(1, nameof(ComChannelDescriptorEditorView.Descriptor), d);
            builder.CloseComponent();
        };
    }
}
