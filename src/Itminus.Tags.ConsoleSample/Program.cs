using Itminus.Tags;
using Itminus.Tags.Projects;
using Microsoft.Extensions.Logging;

Console.WriteLine("Hello, World!");


var logfactory = new LoggerFactory();


Console.WriteLine(".");

var channelDescriptors = ChannelsParser.ReadChannels("C:\\Users\\itminus\\Desktop\\tags.proj1\\channels\\index.json");

Console.WriteLine(".");

//var s7runtime = new S7TagRuntime(logfactory);
//s7runtime.Initialize();
//await s7runtime.RunAsync(CancellationToken.None);
//Console.ReadLine();


//var modbusTcpRuntime = new ZLanTagRuntime(logfactory);
//modbusTcpRuntime.Initialize();
//await modbusTcpRuntime.RunAsync(CancellationToken.None);
//Console.ReadLine();


//var modbusTcpRuntime = new ModbusTcpTagRuntime(logfactory);
//modbusTcpRuntime.Initialize();
//await modbusTcpRuntime.RunAsync(CancellationToken.None);
//Console.ReadLine();