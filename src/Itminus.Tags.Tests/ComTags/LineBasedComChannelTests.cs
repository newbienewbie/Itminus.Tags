using Itminus.Tags.ComScanner;
using Itminus.Tags.ComScanner.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.ComTags;

public class LineBasedComChannelTests
{
    private static readonly ILogger<LineBasedComChannel> Logger = NullLogger<LineBasedComChannel>.Instance;

    /// <summary>
    /// 默认 ReadEntireLine 应为 true（保持向后兼容）
    /// </summary>
    [Fact]
    public void ReadEntireLine_DefaultValue_ShouldBeTrue()
    {
        var channel = new LineBasedComChannel("ch", new ComChannelOption { Port = "COM_TEST" }, Logger);
        Assert.True(channel.ReadEntireLine);
    }

    /// <summary>
    /// ChannelName 应通过构造函数正确传递
    /// </summary>
    [Fact]
    public void Constructor_ShouldSetChannelName()
    {
        var channel = new LineBasedComChannel("my-channel", new ComChannelOption { Port = "COM_TEST" }, Logger);
        Assert.Equal("my-channel", channel.ChannelName);
    }

    /// <summary>
    /// Driver 应返回 ComDriverNames.DriverName
    /// </summary>
    [Fact]
    public void Driver_ShouldReturnComDriverName()
    {
        var channel = new LineBasedComChannel("ch", new ComChannelOption { Port = "COM_TEST" }, Logger);
        Assert.Equal(ComDriverNames.DriverName, channel.Driver);
    }

    /// <summary>
    /// EnsureConnectedAsync 通过 SerialPortFactory 注入 Mock → 应打开串口
    /// </summary>
    [Fact]
    public async Task EnsureConnectedAsync_ShouldOpenSerialPort()
    {
        var mockPort = new MockSerialPortHandle();

        var channel = new LineBasedComChannel("ch-open", new ComChannelOption { Port = "COM_TEST" }, Logger);
        channel.SerialPortFactory = _ => mockPort;

        using var cts = new CancellationTokenSource();
        try
        {
            await channel.EnsureConnectedAsync(false, cts.Token);

            Assert.Equal(1, mockPort.OpenCallCount);
            Assert.NotNull(channel.SerialPort);
        }
        finally
        {
            cts.Cancel();
            await channel.DisconnectAsync(CancellationToken.None);
            channel.Dispose();
        }
    }

    /// <summary>
    /// DisconnectAsync 应关闭 SerialPort 并将引用置为 null
    /// </summary>
    [Fact]
    public async Task DisconnectAsync_ShouldCloseSerialPort()
    {
        var mockPort = new MockSerialPortHandle();

        var channel = new LineBasedComChannel("ch-close", new ComChannelOption { Port = "COM_TEST" }, Logger);
        channel.SerialPortFactory = _ => mockPort;

        using var cts = new CancellationTokenSource();
        try
        {
            await channel.EnsureConnectedAsync(false, cts.Token);
            await channel.DisconnectAsync(CancellationToken.None);

            Assert.Equal(1, mockPort.CloseCallCount);
            Assert.Null(channel.SerialPort);
        }
        finally
        {
            cts.Cancel();
            channel.Dispose();
        }
    }

    /// <summary>
    /// NewLine 应被设置到串口句柄上
    /// </summary>
    [Fact]
    public async Task EnsureConnectedAsync_ShouldSetNewLine()
    {
        var mockPort = new MockSerialPortHandle();

        var channel = new LineBasedComChannel(
            "ch-newline",
            new ComChannelOption
            {
                Port = "COM_TEST",
                NewLine = "abcdefg",
            },
            Logger
        );
        channel.SerialPortFactory = _ => mockPort;

        using var cts = new CancellationTokenSource();
        try
        {
            await channel.EnsureConnectedAsync(false, cts.Token);

            Assert.True(mockPort.IsOpen);
            Assert.Equal("abcdefg", mockPort.NewLine);
        }
        finally
        {
            cts.Cancel();
            await channel.DisconnectAsync(CancellationToken.None);
            channel.Dispose();
        }
    }

    /// <summary>
    /// ReadEntireLine=true → ParseDataAsync 应调用 ReadLine()
    /// </summary>
    [Fact]
    public async Task ReadEntireLineTrue_ShouldCallReadLine()
    {
        var mockPort = new MockSerialPortHandle();
        mockPort.ReadLineQueue.Enqueue("scanned-barcode-123");

        var channel = new LineBasedComChannel("ch-line", new ComChannelOption { Port = "COM_TEST" }, Logger);
        channel.SerialPortFactory = _ => mockPort;
        channel.ReadEntireLine = true;

        using var cts = new CancellationTokenSource();
        var received = new TaskCompletionSource<string>();
        channel.DataReceived += (_, data) => received.TrySetResult(data);

        try
        {
            await channel.EnsureConnectedAsync(false, cts.Token);

            var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.Equal("scanned-barcode-123", result);
        }
        finally
        {
            cts.Cancel();
            await channel.DisconnectAsync(CancellationToken.None);
            channel.Dispose();
        }
    }

    /// <summary>
    /// ReadEntireLine=false → ParseDataAsync 应调用 ReadExisting()
    /// </summary>
    [Fact]
    public async Task ReadEntireLineFalse_ShouldCallReadExisting()
    {
        var mockPort = new MockSerialPortHandle();
        mockPort.ReadExistingReturnValue = "raw-buffer-data";

        var channel = new LineBasedComChannel(
            "ch-existing",
            new ComChannelOption
            {
                Port = "COM_TEST",
                ReadEntireLine = false,
            },
            Logger
        );
        channel.SerialPortFactory = _ => mockPort;

        using var cts = new CancellationTokenSource();
        var received = new TaskCompletionSource<string>();
        channel.DataReceived += (_, data) => received.TrySetResult(data);

        try
        {
            await channel.EnsureConnectedAsync(false, cts.Token);

            var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.Equal("raw-buffer-data", result);
        }
        finally
        {
            cts.Cancel();
            await channel.DisconnectAsync(CancellationToken.None);
            channel.Dispose();
        }
    }

    /// <summary>
    /// WriteAsync 应调用串口句柄的 Write(string)
    /// </summary>
    [Fact]
    public async Task WriteAsync_ShouldCallSerialPortWrite()
    {
        var mockPort = new MockSerialPortHandle();

        var channel = new LineBasedComChannel("ch-write", new ComChannelOption { Port = "COM_TEST" }, Logger);
        channel.SerialPortFactory = _ => mockPort;

        using var cts = new CancellationTokenSource();
        try
        {
            await channel.EnsureConnectedAsync(false, cts.Token);
            await channel.WriteAsync("AT+CMD\r");

            Assert.Equal(1, mockPort.WriteStringCallCount);
            Assert.Equal("AT+CMD\r", mockPort.LastWrittenString);
        }
        finally
        {
            cts.Cancel();
            await channel.DisconnectAsync(CancellationToken.None);
            channel.Dispose();
        }
    }

    /// <summary>
    /// DisconnectAsync 在未连接时应安全执行，不抛异常
    /// </summary>
    [Fact]
    public async Task DisconnectAsync_WhenNotConnected_ShouldNotThrow()
    {
        var channel = new LineBasedComChannel("ch-safe", new ComChannelOption { Port = "COM_TEST" }, Logger);
        await channel.DisconnectAsync(CancellationToken.None);
        Assert.Null(channel.SerialPort);
    }

    /// <summary>
    /// Dispose 在未连接时应安全执行，不抛异常
    /// </summary>
    [Fact]
    public void Dispose_WhenNotConnected_ShouldNotThrow()
    {
        var channel = new LineBasedComChannel("ch-safe", new ComChannelOption { Port = "COM_TEST" }, Logger);
        channel.Dispose();
    }
}
