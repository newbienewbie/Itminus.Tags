using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Editing;
using Itminus.Tags.S7;
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.S7;

sealed class S7TagChannelDescriptorViewer : ITagChannelDescriptorViewer
{
    public int Priority => 100;
    public bool CanView(TagChannelDescriptor descriptor) => descriptor.Driver == S7Names.DriverName;

    public RenderFragment View(TagChannelDescriptor descriptor)
    {
        var d = descriptor.ToS7TagChannelDescriptor();
        return builder =>
        {
            builder.OpenComponent<S7TagChannelDescriptorViewerView>(0);
            builder.AddAttribute(1, nameof(S7TagChannelDescriptorViewerView.Descriptor), d);
            builder.CloseComponent();
        };
    }
}
