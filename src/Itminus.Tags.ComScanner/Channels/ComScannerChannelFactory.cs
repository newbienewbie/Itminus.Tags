using Microsoft.Extensions.Logging;

namespace Itminus.Tags.ComScanner.Channels;

internal class ComScannerChannelFactory : ITagChannelFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public ComScannerChannelFactory(ILoggerFactory loggerFactory)
    {
        this._loggerFactory = loggerFactory;
    }


    public ITagChannel Create(TagChannelDescriptor chDescriptor)
    {
        var descriptor = chDescriptor.ToComScannerTagChannelDescriptor();

        var logger = _loggerFactory.CreateLogger<ComScannerChannel>();
        return new ComScannerChannel(
            descriptor.Name,
            descriptor.Option,
            logger
        );
    }


    private static IReadOnlyList<string> _drivers = new List<string>() { ComScannerNames.DriverName };


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IReadOnlyList<string> GetAvailableDrivers() => _drivers;
}