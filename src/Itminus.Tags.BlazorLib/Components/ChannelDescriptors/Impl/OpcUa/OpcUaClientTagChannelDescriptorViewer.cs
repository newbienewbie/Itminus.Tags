using Itminus.Tags;
using Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Editing;
using Itminus.Tags.OpcUaClient;
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.ChannelDescriptors.Impl.OpcUa;

public sealed class OpcUaClientTagChannelDescriptorViewer : ITagChannelDescriptorViewer
{
    public int Priority => 100;
    public bool CanView(TagChannelDescriptor descriptor) => descriptor.Driver == OpcUaClientNames.DriverName;

    public RenderFragment View(TagChannelDescriptor descriptor)
    {
        var d = descriptor.ToOpcUaClientTagChannelDescriptor();
        return builder =>
        {
            builder.OpenComponent<OpcUaClientTagChannelDescriptorViewerView>(0);
            builder.AddAttribute(1, nameof(OpcUaClientTagChannelDescriptorViewerView.Descriptor), d);
            builder.CloseComponent();
        };
    }
}
