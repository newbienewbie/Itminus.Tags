
using Microsoft.Extensions.Logging;

namespace Itminus.Tags.SimpleFiles;

/// <summary>
/// SimpleFiles通道实现
/// </summary>
internal class SimpleFilesTagChannel : ITagChannel
{
    /// <summary>
    /// c'tor
    /// </summary>
    public SimpleFilesTagChannel(string channelName, SimpleFilesSettings settings, ILogger<SimpleFilesTagChannel> logger)
    {
        if (string.IsNullOrEmpty(channelName))
        {
            throw new ArgumentException($"'{nameof(channelName)}' cannot be null or empty", nameof(channelName));
        }

        this.ChannelName = channelName;
        this.Settings = settings;
        this._logger = logger;
    }

    private readonly ILogger<SimpleFilesTagChannel> _logger;

    /// <inheritdoc/>
    public string ChannelName { get; set; } = SimpleFilesNames.DriverName;

    /// <summary>
    /// 通道设置
    /// </summary>
    public SimpleFilesSettings Settings { get; }

    /// <inheritdoc/>
    public string Driver => SimpleFilesNames.DriverName;

    /// <inheritdoc/>
    public virtual Task DisconnectAsync(CancellationToken ct)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual Task EnsureConnectedAsync(bool force, CancellationToken ct) 
        => Task.CompletedTask;

    /// <summary>
    /// 获取文件路径
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public string MakePath(string address)
    {
        var path = address;
        if (!string.IsNullOrEmpty(this.Settings.BaseDir))
        {
            path = Path.Combine(this.Settings.BaseDir, address);
        }
        return path;
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
    }
}