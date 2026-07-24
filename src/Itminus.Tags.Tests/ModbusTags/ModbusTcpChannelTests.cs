using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itminus.Tags.ModbusTcp;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NModbus;
using Xunit;

namespace Itminus.Tags.Tests.ModbusTags;

/// <summary>
/// 测试用 ModbusTcpChannel，重写 <see cref="ModbusTcpChannel.CreateConnectionAsync"/>
/// 以返回 Moq 创建的 <see cref="IModbusMaster"/>。
/// </summary>
internal class TestModbusTcpChannel : ModbusTcpChannel
{
    public Mock<IModbusMaster> MasterMock { get; }

    public TestModbusTcpChannel(string channelName, Mock<IModbusMaster> masterMock)
        : base(channelName, new ModbusTcpItem(), NullLogger<ModbusTcpChannel>.Instance)
    {
        MasterMock = masterMock;
    }

    protected override Task<IModbusMaster> CreateConnectionAsync(int timeout, CancellationToken ct)
        => Task.FromResult(MasterMock.Object);
}

public class ModbusTcpChannelTests
{
    private static (TestModbusTcpChannel channel, Mock<IModbusMaster> mock) CreateChannel()
    {
        var mock = new Mock<IModbusMaster>(MockBehavior.Strict);
        var channel = new TestModbusTcpChannel("mb1", mock);
        return (channel, mock);
    }

    #region ReadAsync — HoldingRegisters (address ~40001)

    [Fact]
    public async Task ReadAsync_HoldingRegisters_ReturnsBytes()
    {
        var (channel, mock) = CreateChannel();
        mock
            .Setup(x => x.ReadHoldingRegistersAsync(1, (ushort)0, (ushort)2))
            .ReturnsAsync(new ushort[] { 0x1234, 0x5678 });
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        // UShortsToBytes 使用小端序: 0x1234 → [0x34, 0x12], 0x5678 → [0x78, 0x56]
        var result = await channel.ReadAsync("1~40001", 4, CancellationToken.None);

        Assert.Equal(new byte[] { 0x34, 0x12, 0x78, 0x56 }, result);
    }

    [Fact]
    public async Task ReadAsync_HoldingRegisters_OddCount_Throws()
    {
        var (channel, _) = CreateChannel();
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            channel.ReadAsync("1~40001", 3, CancellationToken.None));
        Assert.Contains("偶数", ex.Message);
    }

    #endregion

    #region ReadAsync — InputRegisters (address ~30001)

    [Fact]
    public async Task ReadAsync_InputRegisters_ReturnsBytes()
    {
        var (channel, mock) = CreateChannel();
        mock
            .Setup(x => x.ReadInputRegistersAsync(1, (ushort)0, (ushort)1))
            .ReturnsAsync(new ushort[] { 0xABCD });
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        // 0xABCD → [0xCD, 0xAB]
        var result = await channel.ReadAsync("1~30001", 2, CancellationToken.None);

        Assert.Equal(new byte[] { 0xCD, 0xAB }, result);
    }

    [Fact]
    public async Task ReadAsync_InputRegisters_OddCount_Throws()
    {
        var (channel, _) = CreateChannel();
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            channel.ReadAsync("1~30001", 1, CancellationToken.None));
        Assert.Contains("偶数", ex.Message);
    }

    #endregion

    #region ReadAsync — InputContacts (address ~10001)

    [Fact]
    public async Task ReadAsync_InputContacts_ReturnsBytes()
    {
        var (channel, mock) = CreateChannel();
        mock
            .Setup(x => x.ReadInputsAsync(1, (ushort)0, (ushort)3))
            .ReturnsAsync(new bool[] { true, false, true });
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var result = await channel.ReadAsync("1~10001", 3, CancellationToken.None);

        Assert.Equal(new byte[] { 0x01, 0x00, 0x01 }, result);
    }

    #endregion

    #region ReadAsync — OutputCoils (address ~00001)

    [Fact]
    public async Task ReadAsync_OutputCoils_ReturnsBytes()
    {
        var (channel, mock) = CreateChannel();
        mock
            .Setup(x => x.ReadCoilsAsync(1, (ushort)0, (ushort)2))
            .ReturnsAsync(new bool[] { false, true });
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var result = await channel.ReadAsync("1~00001", 2, CancellationToken.None);

        Assert.Equal(new byte[] { 0x00, 0x01 }, result);
    }

    #endregion

    #region WriteAsync — HoldingRegisters

    [Fact]
    public async Task WriteAsync_HoldingRegisters_WritesBytes()
    {
        var (channel, mock) = CreateChannel();
        ushort[]? captured = null;
        mock
            .Setup(x => x.WriteMultipleRegistersAsync((byte)1, (ushort)0, It.IsAny<ushort[]>()))
            .Returns(Task.CompletedTask)
            .Callback<byte, ushort, ushort[]>((_, _, data) => captured = data);
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        await channel.WriteAsync("1~40001", new byte[] { 0x01, 0x02, 0x03, 0x04 }, CancellationToken.None);

        // BytesToUShorts: [0x01,0x02,0x03,0x04] → [0x0201, 0x0403] (小端)
        Assert.NotNull(captured);
        Assert.Equal(2, captured.Length);
        Assert.Equal((ushort)0x0201, captured[0]);
        Assert.Equal((ushort)0x0403, captured[1]);
    }

    [Fact]
    public async Task WriteAsync_HoldingRegisters_EmptyBytes_DoesNothing()
    {
        var (channel, mock) = CreateChannel();
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        await channel.WriteAsync("1~40001", Array.Empty<byte>(), CancellationToken.None);

        mock.Verify(x => x.WriteMultipleRegistersAsync(It.IsAny<byte>(), It.IsAny<ushort>(), It.IsAny<ushort[]>()), Times.Never);
    }

    [Fact]
    public async Task WriteAsync_HoldingRegisters_LargePayload_SplitsIntoChunks()
    {
        var (channel, mock) = CreateChannel();
        // 124 ushorts (248 bytes) → 超过 123 chunk 限制
        var offsets = new List<ushort>();
        var lengths = new List<int>();
        mock
            .Setup(x => x.WriteMultipleRegistersAsync((byte)1, It.IsAny<ushort>(), It.IsAny<ushort[]>()))
            .Returns(Task.CompletedTask)
            .Callback<byte, ushort, ushort[]>((_, start, data) =>
            {
                offsets.Add(start);
                lengths.Add(data.Length);
            });
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var bytes = new byte[248];
        for (int i = 0; i < bytes.Length; i++) bytes[i] = (byte)(i % 256);

        await channel.WriteAsync("1~40001", bytes, CancellationToken.None);

        // 第一次写入 123 个 ushort，第二次写入剩余的 1 个
        Assert.Equal(2, offsets.Count);
        Assert.Equal(0, offsets[0]);
        Assert.Equal(123, lengths[0]);
        Assert.Equal(123, offsets[1]);
        Assert.Equal(1, lengths[1]);
    }

    #endregion

    #region WriteAsync — OutputCoils

    [Fact]
    public async Task WriteAsync_OutputCoils_WritesBools()
    {
        var (channel, mock) = CreateChannel();
        bool[]? captured = null;
        mock
            .Setup(x => x.WriteMultipleCoilsAsync((byte)1, (ushort)0, It.IsAny<bool[]>()))
            .Returns(Task.CompletedTask)
            .Callback<byte, ushort, bool[]>((_, _, data) => captured = data);
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        await channel.WriteAsync("1~00001", new byte[] { 0x01, 0x00, 0x01 }, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(new bool[] { true, false, true }, captured);
    }

    #endregion

    #region Unsupported areas

    [Fact]
    public async Task WriteAsync_InputRegisters_Throws()
    {
        var (channel, _) = CreateChannel();
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        await Assert.ThrowsAsync<NotImplementedException>(() =>
            channel.WriteAsync("1~30001", new byte[] { 0x01 }, CancellationToken.None));
    }

    [Fact]
    public async Task WriteAsync_InputContacts_Throws()
    {
        var (channel, _) = CreateChannel();
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        await Assert.ThrowsAsync<NotImplementedException>(() =>
            channel.WriteAsync("1~10001", new byte[] { 0x01 }, CancellationToken.None));
    }

    #endregion
}
