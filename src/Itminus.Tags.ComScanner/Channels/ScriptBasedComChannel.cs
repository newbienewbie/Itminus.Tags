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
        this.ReadScriptEmitDebugInformationEnabled = opt.ReadScriptDebugInformationEnabled;
    }

    public string? ReadScript { get; } = "return SerialPort.ReadLine();";

    public override string Driver => ComDriverNames.DriverName;

    ScriptRunner<string>? _runner;

    public bool ReadScriptEmitDebugInformationEnabled { get; } 

    private string? _oldScriptPath;

    protected override async Task<string> ParseDataAsync(SerialPort serialPort, CancellationToken ct)
    {
        if(this._runner is null)
        {
            var scriptOptions = ScriptOptions.Default
                    .AddReferences(typeof(SerialPort).Assembly)
                    .WithImports("System.IO.Ports");
            if(this.ReadScriptEmitDebugInformationEnabled)
            {
                TryClearOldScriptPath();
                var tempPath = Path.GetTempPath();
                var tempFileName = Path.Combine(tempPath, $"Itminus.Tags.COM.{Guid.NewGuid()}.csx");
                var encoding = System.Text.Encoding.UTF8;
                await File.WriteAllTextAsync(tempFileName, this.ReadScript, encoding, ct);
                this._oldScriptPath = tempFileName;
                scriptOptions = scriptOptions
                    .WithEmitDebugInformation(true)
                    .WithFilePath(tempFileName)
                    .WithFileEncoding(encoding);
            }

            var script = CSharpScript.Create<string>(
                this.ReadScript,
                scriptOptions,
                globalsType: typeof(SerialPortGlobals)
            );
            this._runner = script.CreateDelegate();
        }
        var str = await this._runner.Invoke(new SerialPortGlobals(serialPort), ct);
        return str;
    }

    private void TryClearOldScriptPath()
    {
        try
        {
            if(string.IsNullOrEmpty(this._oldScriptPath))
            {
                return;
            }

            if(!File.Exists(this._oldScriptPath))
            {
                return;
            }

            File.Delete(this._oldScriptPath);
        }
        catch 
        {
            // ignore any exception
        }
    }

    public sealed record SerialPortGlobals(SerialPort SerialPort);

    public override void Dispose()
    {
        this.TryClearOldScriptPath();
        base.Dispose();
    }
}
