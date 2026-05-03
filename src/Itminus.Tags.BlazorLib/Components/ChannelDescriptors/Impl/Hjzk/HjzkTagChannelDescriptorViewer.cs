using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Editing;
using Itminus.Tags.Hjzk;
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.Hjzk;

sealed class HjzkTagChannelDescriptorViewer : ITagChannelDescriptorViewer
{
    public int Priority => 110;
    public bool CanView(TagChannelDescriptor descriptor) => descriptor.Driver == HjzkNames.DriverName;

    public RenderFragment View(TagChannelDescriptor descriptor)
    {
        var d = descriptor.ToHjzkTagChannelDescriptor();
        return builder =>
        {
            builder.OpenComponent<HjzkTagChannelDescriptorViewerView>(0);
            builder.AddAttribute(1, nameof(HjzkTagChannelDescriptorViewerView.Descriptor), d);
            builder.CloseComponent();
        };
    }
}
