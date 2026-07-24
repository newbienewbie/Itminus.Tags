using Itminus.Tags.ModbusTcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.ModbusTags;

public class ModbusTcpDirectTagTests
{
    private readonly ServiceProvider _root;

    public ModbusTcpDirectTagTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTagsProjectServices(b =>
        {
            b.AddModbusTcpSupport();
        });

        this._root = services.BuildServiceProvider();
    }

    [Fact]
    public void BuildProject_WithDirectTagsUnderTagGrp_Works()
    {
        using var scope = this._root.CreateScope();
        var sp = scope.ServiceProvider;
        var factory = sp.GetRequiredService<ITagsProjectFactory>();

        var loc = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var dir = Path.GetDirectoryName(loc)!;
        dir = Path.Combine(dir, "ModbusTags", "DirectTags");

        using var proj = factory.Create(dir);

        Assert.Single(proj.Channels);
        Assert.IsType<ModbusTcpChannel>(proj.Channels[0]);

        var grp = proj.Tags.SelectGrp("g-direct");
        Assert.NotNull(grp);

        var bit = grp.SelectTag("bit-v");
        Assert.Equal(BuiltinTagKinds.BIT, bit.TagKind());
        Assert.Equal("1~40021.7", bit.NormalizedAddress());

        var byteTag = grp.SelectTag("byte-v");
        Assert.Equal(BuiltinTagKinds.BYTE, byteTag.TagKind());
        Assert.Equal("1~40020", byteTag.NormalizedAddress());

        var u32 = grp.SelectTag("u32-v");
        Assert.Equal(BuiltinTagKinds.UINT32, u32.TagKind());
        Assert.Equal("1~40022", u32.NormalizedAddress());
        Assert.IsType<UInt32DirectTag>(u32);

        u32.Value = 3000000000u;
        Assert.Equal(3000000000u, u32.Value);

        var i16 = grp.SelectTag("i16-v");
        Assert.Equal(BuiltinTagKinds.INT16, i16.TagKind());
        Assert.Equal("1~40024", i16.NormalizedAddress());
    }

    [Fact]
    public void UInt32DirectTag_AcceptsValuesGreaterThanIntMaxValue()
    {
        var channel = new ModbusTcpChannel(
            "ModbusTcp-1",
            new ModbusTcpItem(),
            new LoggerFactory().CreateLogger<ModbusTcpChannel>());

        var grp = new TagGrp(new TagGrpDescriptor { Name = "grp", IsEntry = true }, channel);
        var container = TagContainer.From(grp);
        var descriptor = new TagDescriptor
        {
            TagName = "u32-v",
            RawAddress = "1~40022",
            TagKind = BuiltinTagKinds.UINT32,
        };

        var tag = new UInt32DirectTag(descriptor, channel, container);
        
        // 测试溢出
        uint newvalue = (uint)int.MaxValue + 1;
        tag.Value = newvalue;

        Assert.Equal(newvalue, tag.Value);
        Assert.True(tag.IsDirty);
    }

    [Fact]
    public async Task UInt32DirectTag_ReadAsync_UsesFakeChannelBytes()
    {
        var channel = new FakeModbusTcpTagChannel();
        var grp = new TagGrp(new TagGrpDescriptor { Name = "grp", IsEntry = true }, channel);
        var container = TagContainer.From(grp);
        var descriptor = new TagDescriptor
        {
            TagName = "u32-v",
            RawAddress = "1~40022",
            TagKind = BuiltinTagKinds.UINT32,
            EndianKind = EndianKinds.BigEndian,
        };

        channel.SetReadPayload("1~40022", new byte[] { 0xB2, 0xD0, 0x5E, 0x00 });
        var tag = new UInt32DirectTag(descriptor, channel, container);

        await tag.ReadAsync(CancellationToken.None);

        Assert.Equal(3000000000u, tag.Value);
    }

    [Fact]
    public async Task UInt32DirectTag_WriteAsync_WritesExpectedBigEndianBytes()
    {
        var channel = new FakeModbusTcpTagChannel();
        var grp = new TagGrp(new TagGrpDescriptor { Name = "grp", IsEntry = true }, channel);
        var container = TagContainer.From(grp);
        var descriptor = new TagDescriptor
        {
            TagName = "u32-v",
            RawAddress = "1~40022",
            TagKind = BuiltinTagKinds.UINT32,
            EndianKind = EndianKinds.BigEndian,
        };

        var tag = new UInt32DirectTag(descriptor, channel, container)
        {
            Value = 3000000000u,
        };

        await tag.WriteAsync(CancellationToken.None);

        Assert.True(channel.TryGetLastWrite("1~40022", out var payload));
        Assert.NotNull(payload);
        Assert.Equal(new byte[] { 0xB2, 0xD0, 0x5E, 0x00 }, payload!);
    }

    [Fact]
    public async Task Int16DirectTag_ReadWrite_WithLittleEndian_Works()
    {
        var channel = new FakeModbusTcpTagChannel();
        var grp = new TagGrp(new TagGrpDescriptor { Name = "grp", IsEntry = true }, channel);
        var container = TagContainer.From(grp);
        var descriptor = new TagDescriptor
        {
            TagName = "i16-v",
            RawAddress = "1~40030",
            TagKind = BuiltinTagKinds.INT16,
            EndianKind = EndianKinds.LittleEndian,
        };

        channel.SetReadPayload("1~40030", new byte[] { 0x34, 0x12 });
        var tag = new Int16DirectTag(descriptor, channel, container);

        await tag.ReadAsync(CancellationToken.None);
        Assert.Equal((short)0x1234, tag.Value);

        tag.Value = -12345;
        await tag.WriteAsync(CancellationToken.None);

        Assert.True(channel.TryGetLastWrite("1~40030", out var payload));
        Assert.Equal(BitConverter.GetBytes((short)-12345), payload);
    }

    private sealed class FakeModbusTcpTagChannel : ModbusTcpChannel
    {
        public FakeModbusTcpTagChannel()
            : base(
                "fake-modbus",
                new ModbusTcpItem(),
                new LoggerFactory().CreateLogger<ModbusTcpChannel>())
        {
        }

        private readonly Dictionary<string, byte[]> _reads = new();
        private readonly Dictionary<string, byte[]> _writes = new();

        public void SetReadPayload(string address, byte[] bytes)
        {
            _reads[address] = (byte[])bytes.Clone();
        }

        public bool TryGetLastWrite(string address, out byte[]? bytes)
        {
            if (_writes.TryGetValue(address, out var got))
            {
                bytes = (byte[])got.Clone();
                return true;
            }

            bytes = null;
            return false;
        }

        public override Task<byte[]> ReadAsync(string address, int count, CancellationToken ct)
        {
            if (!_reads.TryGetValue(address, out var payload))
            {
                throw new InvalidOperationException($"No fake read payload configured for address={address}");
            }

            if (payload.Length != count)
            {
                throw new InvalidOperationException($"Fake payload size mismatch: address={address}, expected={count}, actual={payload.Length}");
            }

            return Task.FromResult((byte[])payload.Clone());
        }

        public override Task WriteAsync(string address, byte[] bytes, CancellationToken ct)
        {
            _writes[address] = (byte[])bytes.Clone();
            return Task.CompletedTask;
        }
    }
}
