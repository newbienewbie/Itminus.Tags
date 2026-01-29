using Itminus.Tags;
using Itminus.Tags.ConsoleSample;
using Itminus.Tags.ModbusTcp;
using Itminus.Tags.Plugins;
using Itminus.Tags.Projects;
using Itminus.Tags.S7;
using Itminus.Tags.ZLan;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

Console.WriteLine(".");

var services = new ServiceCollection();
services.AddLogging();
services.AddTagsProjectServices(b =>
{
    b.Services.AddKeyedSingleton<IChannelFactory, S7TagChannelFactory>("S7");
    b.Services.AddKeyedSingleton<IChannelFactory, ModbusTcpChannelFactory>("ModbusTcp");
    b.Services.AddKeyedSingleton<IChannelFactory, ZLanTcpChannelFactory>("ZLanTcp");

    b.ConfigChannelsFactory((sp, factory) => {
        factory.AddFactory(sp.GetRequiredKeyedService<IChannelFactory>("S7"));
        factory.AddFactory(sp.GetRequiredKeyedService<IChannelFactory>("ModbusTcp"));
        factory.AddFactory(sp.GetRequiredKeyedService<IChannelFactory>("ZLanTcp"));
    });

    b.ConfigTagsLoader((sp, loader) => {
        loader.AddTagsCbntBuilder<S7TagCbntBuilder>(sp, "S7");
        loader.AddTagsCbntBuilder<ModbusTcpTagCbntBuilder>(sp, "ModbusTcp");
        loader.AddTagsCbntBuilder<ZLanDICbntBuilder>(sp, "ZLanTcp", (cbntBuilder) => cbntBuilder.Area.StartsWith("DI"));
        loader.AddTagsCbntBuilder<ZLanDOCbntBuilder>(sp, "ZLanTcp", (cbntBuilder) => cbntBuilder.Area.StartsWith("DO"));
    });
});
var sp = services.BuildServiceProvider();


// 构建 project
var factory = sp.GetRequiredService<ITagsProjectFactory>();
var loc = Assembly.GetExecutingAssembly().Location;
var dir = Path.GetDirectoryName(loc);
var project = factory.Create(dir!);
project.TurnCrashed += (grp, ch, ex) => {

    Console.WriteLine($"{ex.Message}");
    return Task.CompletedTask;
};


// (可选)在运行之前，可以手动调整 Logicets，
//     比如这里移除配置文件中dll，改用代码编写的
project.Logicets.Clear();
project.TryAddLogicet<HandleSnap11>();
project.TryAddLogicet<HandleSnap12>();
project.TryAddLogicet<HandleSnap13>();
// 运行 project
var cts = new CancellationTokenSource();
var task = project.RunAsync(cts.Token);

Console.ReadLine();
cts.Cancel();
await task;