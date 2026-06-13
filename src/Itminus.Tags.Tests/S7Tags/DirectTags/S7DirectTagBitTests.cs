using Itminus.Tags.S7;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.S7Tags;

public class S7DirectTagBitTests
{

    [Theory]
    [InlineData((byte)0, 1, new byte[] { 0b00000001 }, true)]
    [InlineData((byte)7, 1, new byte[] { 0b10000000 }, true)]
    [InlineData((byte)8, 2, new byte[] { 0b00000000, 0b00000001 }, true)]
    [InlineData((byte)9, 2, new byte[] { 0b00000000, 0b00000010 }, true)]
    public async Task DirectBitTag_AutoBufferSize_ReadsEnoughBytes(byte nthBit, int expectedLength, byte[] payload, bool expectedValue)
    {
        var fake = new FakeContinousBytesChannel(payload);
        var tag = CreateBitDirectTag(nthBit, fake);

        await tag.ReadAsync(CancellationToken.None);

        Assert.Equal(expectedLength, fake.LastReadLength);
        Assert.IsType<bool>(tag.Value);
        Assert.Equal(expectedValue,tag.Value);
    }

    [Theory]
    [InlineData((byte)0, true, new byte[] { 0b00000001 })]
    [InlineData((byte)7, true, new byte[] { 0b10000000 })]
    [InlineData((byte)8, true, new byte[] { 0b00000000, 0b00000001 })]
    [InlineData((byte)9, true, new byte[] { 0b00000000, 0b00000010 })]
    [InlineData((byte)0, false, new byte[] { 0b00000000 })]
    [InlineData((byte)8, false, new byte[] { 0b00000000, 0b00000000 })]
    public async Task DirectBitTag_WriteAsync_WritesExpectedFlags(byte nthBit, bool val, byte[] expected)
    {
        var fake = new FakeContinousBytesChannel(new byte[expected.Length]);
        var tag = CreateBitDirectTag(nthBit, fake);

        tag.Value = val;
        await tag.WriteAsync(CancellationToken.None);

        Assert.NotNull(fake.LastWriteBuffer);
        Assert.Equal(expected.Length, fake.LastWriteBuffer!.Length);
        Assert.Equal(expected, fake.LastWriteBuffer);
    }

    #region 测试写操作时，其他位的值是否被正确保留
    [Fact]
    public async Task DirectBitTag_WriteAsync_PreservesOtherBits_ClearOneBit()
    {
        var initial = new byte[] { 0b11111111 };
        var fake = new FakeContinousBytesChannel(initial);
        var tag = CreateBitDirectTag(0, fake); // clear LSB

        tag.Value = false;
        await tag.WriteAsync(CancellationToken.None);

        Assert.NotNull(fake.LastWriteBuffer);
        Assert.Equal(new byte[] { 0b11111110 }, fake.LastWriteBuffer);
    }

    [Fact]
    public async Task DirectBitTag_WriteAsync_PreservesOtherBits_SetOneBit()
    {
        var initial = new byte[] { 0b11111110 };
        var fake = new FakeContinousBytesChannel(initial);
        var tag = CreateBitDirectTag(0, fake); // set LSB

        tag.Value = true;
        await tag.WriteAsync(CancellationToken.None);

        Assert.NotNull(fake.LastWriteBuffer);
        Assert.Equal(new byte[] { 0b11111111 }, fake.LastWriteBuffer);
    }
    #endregion

    private static ITag CreateBitDirectTag(byte nthBit, IContinousBytesBasedTagChannel channel)
    {
        var descriptor = new TagDescriptor()
        {
            TagName = "direct-bit",
            TagKind = BuiltinTagKinds.BIT,
            RawAddress = $"DB1.100.{nthBit}",
        };

        var tag = new BitTag(descriptor, thisChannel: null, channel: channel,nthBit: nthBit, bufferSize: 0);
        return tag;
    }

    private sealed class FakeContinousBytesChannel : IContinousBytesBasedTagChannel
    {
        public FakeContinousBytesChannel(byte[] payload)
        {
            this.LastWriteBuffer = (byte[])payload.Clone();
        }

        public int LastReadLength { get; private set; }

        public byte[] LastWriteBuffer { get; private set; }

        public string ChannelName => "fake";

        public string Driver => S7Names.DriverName;

        public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;

        public Task EnsureConnectedAsync(bool force, CancellationToken ct) => Task.CompletedTask;

        public Task<byte[]> ReadAsync(string address, int count, CancellationToken ct)
        {
            if (count > this.LastWriteBuffer.Length)
            {
                throw new InvalidOperationException($"Unexpected read length={count}");
            }
            this.LastReadLength = count;
            return Task.FromResult(this.LastWriteBuffer.AsSpan(0, count).ToArray());
        }

        public Task WriteAsync(string address, byte[] bytes, CancellationToken ct)
        {
            this.LastWriteBuffer = (byte[])bytes.Clone();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
