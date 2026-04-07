
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.Channels.Editing;

public interface ITagChannelDescriptorViewer
{
    int Priority { get; }
    bool CanView(TagChannelDescriptor descriptor);
    RenderFragment View(TagChannelDescriptor descriptor);
}
