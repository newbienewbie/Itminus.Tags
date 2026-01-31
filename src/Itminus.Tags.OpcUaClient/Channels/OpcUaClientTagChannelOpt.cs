namespace Itminus.Tags.OpcUaClient;

public class OpcUaClientTagChannelOpt
{
    public string ClientName { get; set; } = "ItminusTagsOpcUaClient";
    public OpcUaServerOpt ServerOpt { get; set; } = new();
}

public class OpcUaServerOpt
{
    public string DiscoveryUrl { get; set; } = string.Empty;
    public bool UsePassword { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
