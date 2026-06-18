using Microsoft.Extensions.Logging;
using System.IO.Ports;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Itminus.Tags.ComScanner.Channels;

public class ScriptBasedComChannel : ComChannelBase<string>
{


    public ScriptBasedComChannel(string channelName, ComChannelOption opt, ILogger<ComChannelBase<string>> logger)
        : base(channelName, opt, logger)
    {
        if(!string.IsNullOrWhiteSpace(opt.ReadScript))
        {
            this.ReadScript = opt.ReadScript;
        }
    }

    public string? ReadScript { get; } = "return SerialPort.ReadLine();";

    public override string Driver => ComScannerNames.DriverName;

    ScriptRunner<string>? _runner;

    protected override async Task<string> ParseDataAsync(SerialPort serialPort, CancellationToken ct)
    {
        if(this._runner is null)
        {
            var script = CSharpScript.Create<string>(
                this.ReadScript,
                ScriptOptions.Default
                    .AddReferences(typeof(SerialPort).Assembly)
                    .WithImports("System.IO.Ports"),
                globalsType: typeof(SerialPortGlobals)
            );
            this._runner = script.CreateDelegate();
        }
        var str = await this._runner.Invoke(new SerialPortGlobals(serialPort), ct);
        return str;
    }

    public sealed record SerialPortGlobals(SerialPort SerialPort);
}
