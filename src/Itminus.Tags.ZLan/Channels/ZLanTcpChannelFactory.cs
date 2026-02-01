using Itminus.Tags.ModbusTcp;
using Microsoft.Extensions.Logging;

namespace Itminus.Tags.ZLan;



/// <summary>
/// 构建 <see cref="ZLanChannel"/>
/// </summary>
public class ZLanTcpChannelFactory : ITagChannelFactory
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
    public ITagChannel Create(TagChannelDescriptor descriptor)
    {
        var mbDescriptor = ZLanTcpTagChannelDescriptor.FromDescriptor(descriptor);

        var plcitem = new ModbusTcpItem() { 
            IpAddr = mbDescriptor.IpAddr,
            Port = mbDescriptor.Port,
        };
        var logger = _loggerFactory.CreateLogger<ModbusTcpChannel>();
        return new ZLanTcpChannel(
            descriptor.Name,
            plcitem,
            logger
        );
    }
}