using Itminus.Tags.OpcUaClient;
using Itminus.Tags.Projects;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.OpcUaClientTags;

public class OpcUaClientProjTests
{
    private readonly ServiceProvider _root;

    public OpcUaClientProjTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTagsProjectServices(b =>
        {
            b.AddOpcUaClientSupport();
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
        dir = Path.Combine(dir!, "OpcUaClientTags");
        var proj = factory.Create(dir!);

        // Test Channels
        Assert.Single(proj.Channels);
        Assert.IsType<OpcUaClientTagChannel>(proj.Channels[0]);

        // Test Tags
        var g3 = proj.Tags.SelectGrp("g4");
        Assert.NotNull(g3);


        #region input group
        // Verify Cache Size
        var input = g3.SelectCbnt("输入");
        Assert.IsType<OpcUaClientTagCbnt>(input);
        Assert.Equal(2, input.Children.Count);
        #endregion

        #region input group
        // Verify Cache Size
        var output = g3.SelectCbnt("输出");
        Assert.IsType<OpcUaClientTagCbnt>(output);
        Assert.Equal(4, output.Children.Count);
        #endregion

    }
}
