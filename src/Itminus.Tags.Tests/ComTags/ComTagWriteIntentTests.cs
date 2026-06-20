using Itminus.Tags.ComScanner.Tags;
using Itminus.Tags.ComScanner.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Xml.Linq;
using Xunit;
using Itminus.Tags.ComScanner;

namespace Itminus.Tags.Tests.ComTags;

public class ComTagWriteIntentTests
{
    private const string DriverName = "fake-com-intent";

    [Fact]
    public async Task Should_Write_Message_By_WriteIntent_After_Tag_Read()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var fakeFactory = new FakeComIntentChannelFactory();
        services.AddTagsProjectServices(b =>
        {
            b.AddComScannerSupport();
            b.Services.AddKeyedSingleton<ITagChannelFactory>(DriverName, (_, _) => fakeFactory);
            b.ConfigChannelsFactory((sp, composite) =>
            {
                var factory = sp.GetRequiredKeyedService<ITagChannelFactory>(DriverName);
                composite.AddFactory(factory);
            });
            b.ConfigTagsLoader((sp, composite) => { 
                composite.AddTagBuilder<ComTagBuilder>(DriverName);
            });

        });

        using var root = services.BuildServiceProvider();
        using var scope = root.CreateScope();
        var sp = scope.ServiceProvider;
        var projectFactory = sp.GetRequiredService<ITagsProjectFactory>();

        var rootEle = new XElement("root",
            new XElement("Channel",
                new XAttribute("name", "ch1"),
                new XAttribute("driver", DriverName)
            ),
            new XElement("TagGrp",
                new XAttribute("name", "g"),
                new XAttribute("isEntry", true),
                new XAttribute("isEnabled", true),
                new XAttribute("scanInterval", 10),
                new XElement("Tag",
                    new XAttribute("name", "扫码值"),
                    new XAttribute("channel", "ch1"),
                    new XAttribute("type", "STR")
                )
            )
        );

        var loc = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var dir = Path.GetDirectoryName(loc)!;
        using var proj = projectFactory.Create(dir, rootEle);

        var tag = Assert.IsType<ComReadTag<string>>(proj.Tags.SelectTag("g/扫码值"));
        var ch = Assert.IsType<FakeComIntentChannel>(tag.ComChannel);

        ch.EnqueueIncoming("REQ-001");

        using var cts = new CancellationTokenSource();
        var runTask = Task.Run(() => proj.RunAsync(cts.Token));

        await tag.ReadAsync(cts.Token);
        Assert.Equal("REQ-001", tag.Value);

        var written = proj.WriteIntent("g", async (entry, ct) =>
        {
            var t = Assert.IsType<ComReadTag<string>>(entry.SelectTag("扫码值"));
            var c = Assert.IsType<FakeComIntentChannel>(t.ComChannel);
            await c.WriteMessageForTestAsync($"ACK:{t.Value}");
        }, out var intentTask);

        Assert.True(written);
        await intentTask;

        Assert.Contains("ACK:REQ-001", ch.WrittenMessages);

        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await runTask);
    }


    private sealed class FakeComIntentChannelFactory : ITagChannelFactory
    {
        private static readonly IReadOnlyList<string> Drivers = new[] { DriverName };

        public ITagChannel Create(TagChannelDescriptor chDescriptor)
        {
            return new FakeComIntentChannel(chDescriptor.Name);
        }

        public IReadOnlyList<string> GetAvailableDrivers() => Drivers;
    }

    private sealed class FakeComIntentChannel : ComChannelBase<string>
    {
   

        public FakeComIntentChannel(string name)
            : base(name, new ComChannelOption { ChannelCapacity = 4 }, NullLogger<ComChannelBase<string>>.Instance)
        {
        }

        public override string Driver => DriverName;


        public IList<string> WrittenMessages { get; } = new List<string>();


        public override Task EnsureConnectedAsync(bool force, CancellationToken ct)
        {
            return Task.CompletedTask;  
        }

        public override Task DisconnectAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }


        public override bool TryDequeueInput(out string? input)
        {
            if (!this._channel.Reader.TryRead(out input))
            {
                return false;
            }
            return true;
        }


        public void EnqueueIncoming(string value)
        {
            this._channel.Writer.TryWrite(value);
        }

        public Task WriteMessageForTestAsync(string msg)
        {
            this.WrittenMessages.Add(msg);
            return Task.CompletedTask;
        }

        protected override Task<string> ParseDataAsync(SerialPort sport, CancellationToken ct)
        {
            throw new NotSupportedException("Not used in this test double.");
        }
    }
}
