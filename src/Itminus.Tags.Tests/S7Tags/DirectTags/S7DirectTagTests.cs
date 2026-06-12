using Itminus.Tags.S7;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Itminus.Tags.Tests.S7Tags;

public class S7DirectTagTests
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

    [Fact]
    public async Task DirectStrTag_ReadAsync_ReadsHeaderAndPayload()
    {
        const byte maxLen = 10;
        var payload = new byte[] { maxLen, 5, (byte)'H', (byte)'E', (byte)'L', (byte)'L', (byte)'O', 0, 0, 0, 0, 0 };
        var fake = new FakeContinousBytesChannel(payload);
        var tag = CreateStrDirectTag(maxLen, fake);

        await tag.ReadAsync(CancellationToken.None);
        Assert.Equal(maxLen, tag.Maxlen);
        Assert.Equal(maxLen + 2, fake.LastReadLength);
        Assert.Equal("HELLO", tag.Value);
    }

    [Fact]
    public async Task DirectStrTag_WriteAsync_WritesHeaderAndAscii()
    {
        const byte maxLen = 6;
        var fake = new FakeContinousBytesChannel(new byte[maxLen + 2]);
        var tag = CreateStrDirectTag(maxLen, fake);

        tag.Value = "ABC";
        await tag.WriteAsync(CancellationToken.None);

        Assert.NotNull(fake.LastWriteBuffer);
        Assert.Equal(maxLen, tag.Maxlen);
        Assert.Equal(maxLen + 2, fake.LastWriteBuffer!.Length);
        Assert.Equal(maxLen, fake.LastWriteBuffer[0]);
        Assert.Equal(3, fake.LastWriteBuffer[1]);
        Assert.Equal(new byte[] { (byte)'A', (byte)'B', (byte)'C' }, fake.LastWriteBuffer.AsSpan(2, 3).ToArray());
    }

    [Fact]
    public async Task DirectStrTag_WriteAsync_Throws_WhenInputExceedsMaxLen()
    {
        const byte maxLen = 4;
        var fake = new FakeContinousBytesChannel(new byte[maxLen + 2]);
        var tag = CreateStrDirectTag(maxLen, fake);

        tag.Value = "ABCDE";
        await Assert.ThrowsAsync<ArgumentException>(() => tag.WriteAsync(CancellationToken.None));
    }

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

    private static StrTag CreateStrDirectTag(byte maxLen, IContinousBytesBasedTagChannel channel)
    {
        var descriptor = new TagDescriptor()
        {
            TagName = "direct-str",
            TagKind = BuiltinTagKinds.STR,
            RawAddress = "DB1.300",
        };

        return new StrTag(descriptor, thisChannel: null, channel: channel, maxLen: maxLen);
    }

    private sealed class FakeContinousBytesChannel : IContinousBytesBasedTagChannel
    {
        public FakeContinousBytesChannel(byte[] payload)
        {
            this._payload = payload;
        }

        private readonly byte[] _payload;

        public int LastReadLength { get; private set; }

        public byte[]? LastWriteBuffer { get; private set; }

        public string ChannelName => "fake";

        public string Driver => S7Names.DriverName;

        public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;

        public Task EnsureConnectedAsync(bool force, CancellationToken ct) => Task.CompletedTask;

        public Task<byte[]> ReadAsync(string address, int count, CancellationToken ct)
        {
            this.LastReadLength = count;
            if (count != this._payload.Length)
            {
                throw new InvalidOperationException($"Unexpected read length={count}, expected={this._payload.Length}");
            }
            return Task.FromResult(this._payload);
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
