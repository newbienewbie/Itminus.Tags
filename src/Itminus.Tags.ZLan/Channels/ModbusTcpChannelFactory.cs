using Itminus.Tags.ModbusTcp;
using Microsoft.Extensions.Logging;

namespace Itminus.Tags.ZLan;



/// <summary>
/// 构建 <see cref="ZLanChannel"/>
/// </summary>
public class ZLanTcpChannelFactory : IChannelFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public ZLanTcpChannelFactory(ILoggerFactory loggerFactory)
    {

        this._loggerFactory = loggerFactory;
    }

    private static IReadOnlyList<string> _drivers = new List<string>() { ZLanTcpNames.DriverName };

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
        var mbDescriptor = ModbusTcpTagChannelDescriptor.FromDescriptor(descriptor);

        var plcitem = new ModbusTcpItem() { 
            IpAddr = mbDescriptor.IpAddr,
            Port = mbDescriptor.Port,
        };
        var logger = _loggerFactory.CreateLogger<ModbusTcpChannel>();
        return new ModbusTcpChannel(
            descriptor.Name,
            plcitem,
            logger
        );
    }
}