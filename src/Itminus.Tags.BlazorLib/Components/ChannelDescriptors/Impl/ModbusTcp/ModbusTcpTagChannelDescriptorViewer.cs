using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Editing;
using Itminus.Tags.ModbusTcp;
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.ModbusTcp;

sealed class ModbusTcpTagChannelDescriptorViewer : ITagChannelDescriptorViewer
{
    public int Priority => 100;
    public bool CanView(TagChannelDescriptor descriptor) => descriptor.Driver == ModbusTcpNames.DriverName;

    public RenderFragment View(TagChannelDescriptor descriptor)
    {
        var d = descriptor.ToModbusTcpTagChannelDescriptor();
        return builder =>
        {
            builder.OpenComponent<ModbusTcpTagChannelDescriptorViewerView>(0);
            builder.AddAttribute(1, nameof(ModbusTcpTagChannelDescriptorViewerView.Descriptor), d);
            builder.CloseComponent();
        };
    }
}
