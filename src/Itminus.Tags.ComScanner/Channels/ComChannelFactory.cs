using Microsoft.Extensions.Logging;

namespace Itminus.Tags.ComScanner.Channels;

internal class ComChannelFactory : ITagChannelFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public ComChannelFactory(ILoggerFactory loggerFactory)
    {
        this._loggerFactory = loggerFactory;
    }


    public ITagChannel Create(TagChannelDescriptor chDescriptor)
    {
        var descriptor = chDescriptor.ToComChannelDescriptor();

        if(!string.IsNullOrWhiteSpace(descriptor.Option.ReadScript))
        {
            var scriptLogger = _loggerFactory.CreateLogger<ComChannelBase<string>>();
            return new ScriptBasedComChannel(
                descriptor.Name,
                descriptor.Option,
                scriptLogger
            );
        }

        // 回退到默认的基于行的串口扫描器
        var lineLogger = _loggerFactory.CreateLogger<LineBasedComChannel>();
        return new LineBasedComChannel(
            descriptor.Name,
            descriptor.Option,
            lineLogger
        );
    }


    private static IReadOnlyList<string> _drivers = new List<string>() { 
        ComDriverNames.DriverName,
    };


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IReadOnlyList<string> GetAvailableDrivers() => _drivers;
}