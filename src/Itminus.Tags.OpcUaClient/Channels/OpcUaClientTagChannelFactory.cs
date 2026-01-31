using Microsoft.Extensions.Logging;

namespace Itminus.Tags.OpcUaClient;

public class OpcUaClientTagChannelFactory : IChannelFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public OpcUaClientTagChannelFactory(ILoggerFactory loggerFactory)
    {

        this._loggerFactory = loggerFactory;
    }

    private static IReadOnlyList<string> _drivers = new List<string>() { OpcUaClientNames.DriverName };

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IReadOnlyList<string> GetAvailableDrivers() => _drivers;



    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ITagChannel Create(ChannelDescriptor descriptor)
    {
        var opcDescriptor = OpcUaClientTagChannelDescriptor.FromDescriptor(descriptor);

        var opt = opcDescriptor.OpcUaTagChannelOpt;
        var logger = _loggerFactory.CreateLogger<OpcUaClientTagChannel>();
        return new OpcUaClientTagChannel(
            descriptor.Name,
            opt,
            logger
        );
    }
}
