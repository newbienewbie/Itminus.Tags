using Opc.Ua;
using System.Xml.Linq;

namespace Itminus.Tags.OpcUaClient;

public class OpcUaClientTagChannelDescriptor : ChannelDescriptor
{

    public OpcUaClientTagChannelOpt OpcUaTagChannelOpt { get; set; } = new();

    const string DefaultClientName  = "ItminusTagsOpcUaClient";


    private static OpcUaServerOpt ParseSreverOpt(XElement serverOptEle)
    {
        var discoveryUrl = 
            serverOptEle.Attribute(nameof(OpcUaServerOpt.DiscoveryUrl))?.Value ?? 
            serverOptEle.Element(nameof(OpcUaServerOpt.DiscoveryUrl))?.Value ??
            "localhost";
        var usePasswordStr =
            serverOptEle.Attribute(nameof(OpcUaServerOpt.UsePassword))?.Value ??
            serverOptEle.Element(nameof(OpcUaServerOpt.UsePassword))?.Value ??
            "false";
        var username = 
            serverOptEle.Attribute(nameof(OpcUaServerOpt.UserName))?.Value ??
            serverOptEle.Element(nameof(OpcUaServerOpt.UserName))?.Value ??
            string.Empty;
        var password = 
            serverOptEle.Attribute(nameof(OpcUaServerOpt.Password))?.Value ??
            serverOptEle.Element(nameof(OpcUaServerOpt.Password))?.Value ??
            string.Empty;
        var serverOpt = new OpcUaServerOpt()
        {
            DiscoveryUrl = discoveryUrl,
            UsePassword =
                bool.TryParse(usePasswordStr, out var usePassword ) ?
                usePassword :
                false,
            UserName = username,
            Password = password,
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
            ParseSreverOpt(serverEle) :
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