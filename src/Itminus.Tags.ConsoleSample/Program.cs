using Itminus.Tags;
using Itminus.Tags.ConsoleSample;
using Itminus.Tags.ModbusTcp;
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
    b.AddS7Support();
    b.AddModbusTcpSupport();
    b.AddZLanTcpSupport();
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