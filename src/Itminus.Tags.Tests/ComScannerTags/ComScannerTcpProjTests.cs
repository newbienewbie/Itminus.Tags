using Itminus.Tags.ComScanner;
using Itminus.Tags.ComScanner.Channels;
using Itminus.Tags.ModbusTcp;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.ComScanner;

public class ComScannerTcpProjTests
{
    private readonly ServiceProvider _root;

    public ComScannerTcpProjTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTagsProjectServices(b =>
        {
            b.AddComScannerSupport();
        });

        this._root = services.BuildServiceProvider();
    }

    [Fact]
    public void TestLoad()
    {
        var scope = this._root.CreateScope();
        var sp = scope.ServiceProvider;
        var factory = sp.GetRequiredService<ITagsProjectFactory>();
        var loc = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var dir = Path.GetDirectoryName(loc);
        dir = Path.Combine(dir!, "ComScannerTags");
        var proj = factory.Create(dir!);

        // Test Channels
        Assert.Equal(2, proj.Channels.Count);
        Assert.IsType<ComScannerChannel>(proj.Channels[0]);
        Assert.IsType<ComScannerChannel>(proj.Channels[1]);

        // Test Tags
        var g = proj.Tags.SelectGrp("g");
        Assert.NotNull(g);


        #region input group
        var gun1 = g.SelectTag("1#扫码枪");
        Assert.Equal("1#扫码枪", gun1.TagName());
        Assert.Equal(BuiltinTagKinds.STR, gun1.TagKind());
        Assert.IsType<ComScannerChannel>(gun1.Channel);
        Assert.Equal(proj.Channels[0], gun1.Channel);
        var channel1 = (ComScannerChannel) gun1.Channel;
        Assert.Null(channel1.NewLine);
        Assert.Equal(1, channel1.Capacity);

        var gun2 = g.SelectTag("2#扫码枪");
        Assert.Equal("2#扫码枪", gun2.TagName());
        Assert.Equal(BuiltinTagKinds.STR, gun2.TagKind());
        Assert.IsType<ComScannerChannel>(gun2.Channel);
        Assert.Equal(proj.Channels[1], gun2.Channel);
        var channel2 = (ComScannerChannel)gun2.Channel;
        Assert.Equal("\r\n",channel2.NewLine);
        Assert.Equal(42, channel2.Capacity);
        #endregion


    }
}
