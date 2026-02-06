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
        Assert.Single(proj.Channels);
        Assert.IsType<ComScannerChannel>(proj.Channels[0]);

        // Test Tags
        var g = proj.Tags.SelectGrp("g");
        Assert.NotNull(g);


        #region input group
        var gun = g.SelectTag("1#扫码枪");
        Assert.Equal("1#扫码枪", gun.TagName());
        Assert.Equal(TagKinds.STR, gun.TagKind());
        #endregion


    }
}
