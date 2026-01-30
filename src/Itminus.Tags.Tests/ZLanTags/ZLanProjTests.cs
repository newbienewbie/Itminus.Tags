using Itminus.Tags.Projects;
using Itminus.Tags.ZLan;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.ZLanTags;

public class ZLanProjTests
{
    private readonly ServiceProvider _root;

    public ZLanProjTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTagsProjectServices(b =>
        {
            b.Services.AddKeyedSingleton<IChannelFactory, ZLanTcpChannelFactory>("ZLanTcp");
            b.ConfigChannelsFactory((sp, component) => {
                component.AddFactory(sp.GetRequiredKeyedService<IChannelFactory>("ZLanTcp"));
            });

            b.ConfigTagsLoader((sp, loader) => {
                loader.AddTagsCbntBuilder<ZLanDICbntBuilder>(driver: "ZLanTcp", predicate: b => b.Area == "DI");
                loader.AddTagsCbntBuilder<ZLanDOCbntBuilder>(driver: "ZLanTcp", predicate: b => b.Area == "DO");
            });
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
        dir = Path.Combine(dir!, "ZLanTags");
        var proj = factory.Create(dir!);

        // Test Channels
        Assert.Single(proj.Channels);
        Assert.IsType<ZLanTcpChannel>(proj.Channels[0]);

        // Test Tags
        Assert.NotNull(proj.Tags);
        Assert.Equal("__main__", proj.Tags.Name);
        var g2 = proj.Tags.SelectGrp("g2");
        Assert.NotNull(g2);


        #region input group
        var btnLetGo = g2.SelectTag("输入/放行按钮闭合状态");
        Assert.Equal("放行按钮闭合状态", btnLetGo.TagName());

        var btnManual = g2.SelectTag("输入/手动");
        Assert.Equal("手动", btnManual.TagName());

        // Verify Cache Size
        var input = g2.SelectCbnt("输入");
        Assert.Equal(2, input.CacheSize);
        Assert.Equal(2, input.Cache.Length);
        #endregion

        #region output group
        var ledGreen = g2.SelectTag("输出/绿灯");
        Assert.Equal("绿灯", ledGreen.TagName());

        var ledRed = g2.SelectTag("输出/红灯");
        Assert.Equal("红灯", ledRed.TagName());

        var ledYellow = g2.SelectTag("输出/黄灯");
        Assert.Equal("黄灯", ledYellow.TagName());

        var ledLetGo = g2.SelectTag("输出/放行灯");
        Assert.Equal("放行灯", ledLetGo.TagName());

        // Verify Cache Size
        var output = g2.SelectCbnt("输出");
        Assert.Equal(4, output.CacheSize);
        Assert.Equal(4, output.Cache.Length);
        #endregion
    }
}
