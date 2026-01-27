using Itminus.Tags;
using Itminus.Tags.ModbusTcp;
using Itminus.Tags.Projects;
using Itminus.Tags.S7;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("Hello, World!");


var logfactory = new LoggerFactory();


Console.WriteLine(".");

//var channelDescriptors = ChannelsParser.ReadChannels("C:\\Users\\itminus\\Desktop\\tags.proj1\\channels\\index.json");

Console.WriteLine(".");

//var services = new ServiceCollection();
//services.AddTagsProjectServices(b =>
//{
//    b.Services.AddKeyedSingleton<ITagChannelFactory, S7TagChannelFactory>("S7");
//    b.Services.AddKeyedSingleton<ITagChannelFactory, ModbusTcpChannelFactory>("ModbusTcp");

//    b.ConfigChannelsFactory((sp, factory) => {
//        factory.AddFactory(sp.GetRequiredKeyedService<ITagChannelFactory>("S7"));
//        factory.AddFactory(sp.GetRequiredKeyedService<ITagChannelFactory>("ModbusTcp"));
//    });

//    b.ConfigTagsLoader((sp, loader) => {
//        loader.AddTagsCbntBuilder<S7TagCbntBuilder>(sp, "S7");
//        loader.AddTagsCbntBuilder<ModbusTcpTagCbntBuilder>(sp, "ModbusTcp");
//    });
//});

var s7runtime = new S7TagRuntime(logfactory);
s7runtime.Initialize();
await s7runtime.RunAsync(CancellationToken.None);
Console.ReadLine();


//var modbusTcpRuntime = new ZLanTagRuntime(logfactory);
//modbusTcpRuntime.Initialize();
//await modbusTcpRuntime.RunAsync(CancellationToken.None);
//Console.ReadLine();


//var modbusTcpRuntime = new ModbusTcpTagRuntime(logfactory);
//modbusTcpRuntime.Initialize();
//await modbusTcpRuntime.RunAsync(CancellationToken.None);
//Console.ReadLine();