using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.Channels.Editing;

public interface ITagChannelDescriptorEditor
{
    int Priority { get; }
    bool CanEdit(TagChannelDescriptor descriptor);

    /// <summary>
    /// Render an editor UI for a descriptor. Implementations may materialize a strongly-typed descriptor.
    /// When descriptor instance changes or its values change, call <paramref name="descriptorChanged"/>.
    /// </summary>
    RenderFragment Edit(TagChannelDescriptor descriptor, Action<TagChannelDescriptor> descriptorChanged);
}
