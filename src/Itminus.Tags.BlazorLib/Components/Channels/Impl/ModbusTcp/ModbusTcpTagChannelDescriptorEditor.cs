using Itminus.Tags.BlazorLib.Components.Channels.Editing;
using Itminus.Tags.ModbusTcp;
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.Channels.Impl.ModbusTcp;

sealed class ModbusTcpTagChannelDescriptorEditor : ITagChannelDescriptorEditor
{
    public int Priority => 100;
    public bool CanEdit(TagChannelDescriptor descriptor) => descriptor.Driver == ModbusTcpNames.DriverName;

    public RenderFragment Edit(TagChannelDescriptor descriptor, Action<TagChannelDescriptor> descriptorChanged)
    {
        var d = descriptor.ToModbusTcpTagChannelDescriptor();
        if (!ReferenceEquals(d, descriptor))
        {
            descriptorChanged(d);
        }

        return builder =>
        {
            builder.OpenComponent<ModbusTcpTagChannelDescriptorEditorView>(0);
            builder.AddAttribute(1, nameof(ModbusTcpTagChannelDescriptorEditorView.Descriptor), d);
            builder.CloseComponent();
        };
    }
}
