using Itminus.Tags.S7;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.S7Tags;

public class S7ProjTests
{
    private readonly ServiceProvider _root;

    public S7ProjTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTagsProjectServices(b =>
        {
            b.AddS7Support();
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
        var dir = System.IO.Path.GetDirectoryName(loc);
        dir = Path.Combine(dir!, "S7Tags");
        var proj = factory.Create(dir!);

        // Test Channels
        Assert.Equal(2, proj.Channels.Count);
        Assert.IsType<S7TagChannel>(proj.Channels[0]);
        Assert.IsType<S7TagChannel>(proj.Channels[1]);


        // Test Tags
        var g1 = proj.Tags.SelectGrp("g1");
        Assert.NotNull(g1);


        #region input group
        // Verify Cache Size
        var input = g1.SelectCbnt("拍照请求");
        Assert.Equal(6, input.CacheSize);
        Assert.Equal(6, input.Cache.Length);

        var req = g1.SelectTag("拍照请求/拍照-请求-标志");
        Assert.Equal("拍照-请求-标志", req.TagName());

        var reqMat = g1.SelectTag("拍照请求/拍照-请求-料号");
        Assert.Equal("拍照-请求-料号", reqMat.TagName());

        var reqProg = g1.SelectTag("拍照请求/拍照-请求-程序号");
        Assert.Equal("拍照-请求-程序号", reqProg.TagName());
        Assert.Equal("DB200.104", reqProg.TagAddress());
        Assert.Equal(TagKinds.INT16, reqProg.TagKind());

        #endregion

        #region output group
        // Verify Cache Size
        var output = g1.SelectCbnt("拍照响应");
        Assert.Equal(2, output.CacheSize);
        Assert.Equal(2, output.Cache.Length);

        var ack = g1.SelectTag("拍照响应/拍照-响应-标志");
        Assert.Equal("拍照-响应-标志", ack.TagName());

        var ackB1 = g1.SelectTag("拍照响应/拍照-响应-B1");
        Assert.Equal("拍照-响应-B1", ackB1.TagName());

        var ackI2 = g1.SelectTag("拍照响应/拍照-响应-I2");
        Assert.Equal("拍照-响应-I2", ackI2.TagName());
        Assert.Equal("DB200.400.8", ackI2.TagAddress());
        Assert.Equal(TagKinds.BIT, ackI2.TagKind());
        #endregion
    }
}
