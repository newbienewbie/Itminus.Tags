using Opc.Ua;
using System.Xml.Linq;

namespace Itminus.Tags.OpcUaClient;

public class OpcUaClientTagChannelDescriptor : ChannelDescriptor
{

    public OpcUaClientTagChannelOpt OpcUaTagChannelOpt { get; set; } = new();

    const string DefaultClientName  = "ItminusTagsOpcUaClient";


    private static OpcUaServerOpt ParseSreverOpt(XElement serverOptEle)
    {
        var serverOpt = new OpcUaServerOpt()
        {
            DiscoveryUrl = serverOptEle.Attribute(nameof(OpcUaServerOpt.DiscoveryUrl))?.Value ?? "localhost",
            ScanInterval =
                int.TryParse(
                    serverOptEle.Attribute(nameof(OpcUaServerOpt.ScanInterval))?.Value,
                    out var scanInterval
                ) ?
                scanInterval :
                500,
            UsePassword =
                bool.TryParse(
                    serverOptEle.Attribute(nameof(OpcUaServerOpt.UsePassword))?.Value,
                    out var usePassword
                ) ?
                usePassword :
                false,
            UserName = serverOptEle.Attribute(nameof(OpcUaServerOpt.UserName))?.Value ?? string.Empty,
            Password = serverOptEle.Attribute(nameof(OpcUaServerOpt.Password))?.Value ?? string.Empty,
        };
        return serverOpt;
    }

    public static OpcUaClientTagChannelDescriptor FromDescriptor(ChannelDescriptor descriptor)
    {
        if (descriptor.Driver != OpcUaClientNames.DriverName)
        {
            throw new InvalidOperationException($"通道驱动错误：期望 {OpcUaClientNames.DriverName}，而当前为{descriptor.Driver}");
        }
        if (descriptor is OpcUaClientTagChannelDescriptor d)
        {
            return d;
        }

        var serverOpt = descriptor.Extras.TryGetValue(nameof(OpcUaTagChannelOpt.ServerOpt), out var serverEle) ?
            ParseSreverOpt(XElement.Parse(serverEle.Value)) :
            new OpcUaServerOpt();

        var res = new OpcUaClientTagChannelDescriptor
            {
                Name = descriptor.Name,
                Driver = descriptor.Driver,
                Extras = descriptor.Extras,

                OpcUaTagChannelOpt = new OpcUaClientTagChannelOpt()
                {
                    ClientName = !descriptor.Extras.TryGetValue(nameof(OpcUaTagChannelOpt.ClientName), out var clientName) ?
                        DefaultClientName :
                        clientName.Value ?? DefaultClientName,
                    ServerOpt = serverOpt,
                },
            };
        return res;
    }
}