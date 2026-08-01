using System;
using System.Collections.Generic;
using System.Linq;
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

    public TestModbusTcpChannel(ModbusTcpTagChannelDescriptor descriptor, Mock<IModbusMaster> masterMock)
        : base(descriptor, NullLogger<ModbusTcpChannel>.Instance)
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
        var channel = new TestModbusTcpChannel(new ModbusTcpTagChannelDescriptor { Name = "mb1" }, mock);
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

    [Fact]
    public async Task ReadAsync_HoldingRegisters_OverPduLimit_Batches()
    {
        var (channel, mock) = CreateChannel();
        // 252 字节 = 126 个寄存器 > 125 单帧上限 → 应分 2 批：125 + 1
        mock
            .Setup(x => x.ReadHoldingRegistersAsync(1, (ushort)0, (ushort)125))
            .ReturnsAsync(Enumerable.Range(1, 125).Select(i => (ushort)i).ToArray());
        mock
            .Setup(x => x.ReadHoldingRegistersAsync(1, (ushort)125, (ushort)1))
            .ReturnsAsync(new ushort[] { 0x00FF });
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var result = await channel.ReadAsync("1~40001", 252, CancellationToken.None);

        Assert.Equal(252, result.Length);
        // 共两批调用
        mock.Verify(x => x.ReadHoldingRegistersAsync(It.IsAny<byte>(), It.IsAny<ushort>(), It.IsAny<ushort>()), Times.Exactly(2));
        // 第一批：ushort 1..125，小端序
        for (int i = 0; i < 125; i++)
        {
            var u = (ushort)(i + 1);
            Assert.Equal((byte)(u & 0xFF), result[i * 2]);
            Assert.Equal((byte)(u >> 8), result[i * 2 + 1]);
        }
        // 第二批：0x00FF → [0xFF, 0x00]
        Assert.Equal(0xFF, result[250]);
        Assert.Equal(0x00, result[251]);
    }

    [Fact]
    public async Task ReadAsync_HoldingRegisters_ExactlyPduLimit_SingleCall()
    {
        var (channel, mock) = CreateChannel();
        // 250 字节 = 125 个寄存器 = 恰好单帧上限 → 一次调用
        mock
            .Setup(x => x.ReadHoldingRegistersAsync(1, (ushort)0, (ushort)125))
            .ReturnsAsync(Enumerable.Repeat((ushort)0x0001, 125).ToArray());
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var result = await channel.ReadAsync("1~40001", 250, CancellationToken.None);

        Assert.Equal(250, result.Length);
        mock.Verify(x => x.ReadHoldingRegistersAsync(It.IsAny<byte>(), It.IsAny<ushort>(), It.IsAny<ushort>()), Times.Once);
    }

    [Fact]
    public async Task ReadAsync_HoldingRegisters_CustomMaxReadRegisters_BatchesAtConfiguredSize()
    {
        var mock = new Mock<IModbusMaster>(MockBehavior.Strict);
        var channel = new TestModbusTcpChannel(new ModbusTcpTagChannelDescriptor { Name = "mb1", MaxReadRegisters = 32 }, mock);
        // 设备单帧上限 32 寄存器：读 100 个寄存器 → 32+32+32+4
        mock
            .Setup(x => x.ReadHoldingRegistersAsync(1, (ushort)0, (ushort)32))
            .ReturnsAsync(Enumerable.Repeat((ushort)0x0101, 32).ToArray());
        mock
            .Setup(x => x.ReadHoldingRegistersAsync(1, (ushort)32, (ushort)32))
            .ReturnsAsync(Enumerable.Repeat((ushort)0x0102, 32).ToArray());
        mock
            .Setup(x => x.ReadHoldingRegistersAsync(1, (ushort)64, (ushort)32))
            .ReturnsAsync(Enumerable.Repeat((ushort)0x0103, 32).ToArray());
        mock
            .Setup(x => x.ReadHoldingRegistersAsync(1, (ushort)96, (ushort)4))
            .ReturnsAsync(Enumerable.Repeat((ushort)0x0104, 4).ToArray());
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var result = await channel.ReadAsync("1~40001", 200, CancellationToken.None);

        Assert.Equal(200, result.Length);
        // 每批小端：0x0101 → [0x01, 0x01]
        Assert.Equal(0x01, result[0]);
        Assert.Equal(0x01, result[1]);
        // 末批 0x0104 → [0x04, 0x01]
        Assert.Equal(0x04, result[198]);
        Assert.Equal(0x01, result[199]);
        mock.Verify(x => x.ReadHoldingRegistersAsync(It.IsAny<byte>(), It.IsAny<ushort>(), It.IsAny<ushort>()), Times.Exactly(4));
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

    [Fact]
    public async Task ReadAsync_InputRegisters_OverPduLimit_Batches()
    {
        var (channel, mock) = CreateChannel();
        // 252 字节 = 126 个寄存器 > 125 单帧上限 → 应分 2 批：125 + 1
        mock
            .Setup(x => x.ReadInputRegistersAsync(1, (ushort)0, (ushort)125))
            .ReturnsAsync(Enumerable.Repeat((ushort)0x1111, 125).ToArray());
        mock
            .Setup(x => x.ReadInputRegistersAsync(1, (ushort)125, (ushort)1))
            .ReturnsAsync(new ushort[] { 0x2222 });
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var result = await channel.ReadAsync("1~30001", 252, CancellationToken.None);

        Assert.Equal(252, result.Length);
        // 0x1111 → [0x11, 0x11]
        Assert.Equal(0x11, result[0]);
        Assert.Equal(0x11, result[1]);
        // 0x2222 → [0x22, 0x22]
        Assert.Equal(0x22, result[250]);
        Assert.Equal(0x22, result[251]);
        mock.Verify(x => x.ReadInputRegistersAsync(It.IsAny<byte>(), It.IsAny<ushort>(), It.IsAny<ushort>()), Times.Exactly(2));
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

    [Fact]
    public async Task ReadAsync_InputContacts_OverPduLimit_Batches()
    {
        var (channel, mock) = CreateChannel();
        // 2001 点 > 2000 单帧上限 → 应分 2 批：2000 + 1
        mock
            .Setup(x => x.ReadInputsAsync(1, (ushort)0, (ushort)2000))
            .ReturnsAsync(Enumerable.Repeat(true, 2000).ToArray());
        mock
            .Setup(x => x.ReadInputsAsync(1, (ushort)2000, (ushort)1))
            .ReturnsAsync(new bool[] { false });
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var result = await channel.ReadAsync("1~10001", 2001, CancellationToken.None);

        Assert.Equal(2001, result.Length);
        Assert.All(result.Take(2000), b => Assert.Equal(0x01, b));
        Assert.Equal(0x00, result[2000]);
        mock.Verify(x => x.ReadInputsAsync(It.IsAny<byte>(), It.IsAny<ushort>(), It.IsAny<ushort>()), Times.Exactly(2));
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

    [Fact]
    public async Task ReadAsync_OutputCoils_OverPduLimit_Batches()
    {
        var (channel, mock) = CreateChannel();
        // 2001 点 > 2000 单帧上限 → 应分 2 批：2000 + 1
        mock
            .Setup(x => x.ReadCoilsAsync(1, (ushort)0, (ushort)2000))
            .ReturnsAsync(Enumerable.Repeat(false, 2000).ToArray());
        mock
            .Setup(x => x.ReadCoilsAsync(1, (ushort)2000, (ushort)1))
            .ReturnsAsync(new bool[] { true });
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var result = await channel.ReadAsync("1~00001", 2001, CancellationToken.None);

        Assert.Equal(2001, result.Length);
        Assert.All(result.Take(2000), b => Assert.Equal(0x00, b));
        Assert.Equal(0x01, result[2000]);
        mock.Verify(x => x.ReadCoilsAsync(It.IsAny<byte>(), It.IsAny<ushort>(), It.IsAny<ushort>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ReadAsync_OutputCoils_CustomMaxReadBits_BatchesAtConfiguredSize()
    {
        var mock = new Mock<IModbusMaster>(MockBehavior.Strict);
        var channel = new TestModbusTcpChannel(new ModbusTcpTagChannelDescriptor { Name = "mb1", MaxReadBits = 500 }, mock);
        // 设备单帧上限 500 点：读 1200 点 → 500+500+200
        mock
            .Setup(x => x.ReadCoilsAsync(1, (ushort)0, (ushort)500))
            .ReturnsAsync(Enumerable.Repeat(true, 500).ToArray());
        mock
            .Setup(x => x.ReadCoilsAsync(1, (ushort)500, (ushort)500))
            .ReturnsAsync(Enumerable.Repeat(false, 500).ToArray());
        mock
            .Setup(x => x.ReadCoilsAsync(1, (ushort)1000, (ushort)200))
            .ReturnsAsync(Enumerable.Repeat(true, 200).ToArray());
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        var result = await channel.ReadAsync("1~00001", 1200, CancellationToken.None);

        Assert.Equal(1200, result.Length);
        Assert.Equal(0x01, result[0]);       // 第一批 true
        Assert.Equal(0x00, result[500]);     // 第二批 false
        Assert.Equal(0x01, result[1000]);    // 第三批 true
        mock.Verify(x => x.ReadCoilsAsync(It.IsAny<byte>(), It.IsAny<ushort>(), It.IsAny<ushort>()), Times.Exactly(3));
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

    #region MaxWriteRegisters 自定义

    [Fact]
    public async Task WriteAsync_HoldingRegisters_CustomMaxWriteRegisters_ChunksCorrectly()
    {
        var mock = new Mock<IModbusMaster>(MockBehavior.Strict);
        var item = new ModbusTcpItem { MaxWriteRegisters = 2 };
        var channel = new TestModbusTcpChannel(new ModbusTcpTagChannelDescriptor { Name = "mb1", MaxWriteRegisters = 2 }, mock);
        mock
            .Setup(x => x.WriteMultipleRegistersAsync(
                (byte)1, 
                It.IsAny<ushort>(), 
                It.IsAny<ushort[]>()
            ))
            .Returns(Task.CompletedTask);
        await channel.EnsureConnectedAsync(false, CancellationToken.None);

        // 4 ushorts → 应分 2 批写入（每批 2 个）
        var bytes = new byte[] { 0x01, 0x00, 0x02, 0x00, 0x03, 0x00, 0x04, 0x00 };
        await channel.WriteAsync("1~40001", bytes, CancellationToken.None);

        mock.Verify(
            x => x.WriteMultipleRegistersAsync(1, 0, It.Is<ushort[]>(d => d.Length == 2)),
            Times.Once);
        mock.Verify(
            x => x.WriteMultipleRegistersAsync(1, 2, It.Is<ushort[]>(d => d.Length == 2)),
            Times.Once);
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
