using Microsoft.Extensions.Logging;
using System.IO.Ports;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Itminus.Tags.ComScanner.Channels;


/// <summary>
/// 基于脚本的串口通信通道，每次读取数据时都会执行脚本，脚本中可以使用 serial 变量来访问串口，得到一个类型为T的对象。
/// </summary>
/// <typeparam name="T"></typeparam>
public class ScriptBasedComChannel<T> : ComChannelBase<T>
{


    public ScriptBasedComChannel(string channelName, ComChannelOption opt, ILogger<ComChannelBase<T>> logger)
        : base(channelName, opt, logger)
    {
        if(!string.IsNullOrWhiteSpace(opt.ReadScript))
        {
            this.ReadScript = opt.ReadScript;
        }
        this.ReadScriptEmitDebugInformationEnabled = opt.ReadScriptDebugInformationEnabled;
    }

    public string? ReadScript { get; } = "return serial.ReadLine();";

    public override string Driver => ComDriverNames.DriverName;

    ScriptRunner<T>? _runner;

    public bool ReadScriptEmitDebugInformationEnabled { get; } 

    private string? _oldScriptPath;

    protected override async Task<T> ParseDataAsync(SerialPort serial, CancellationToken ct)
    {
        if(this._runner is null)
        {
            var scriptOptions = ScriptOptions.Default
                    .AddReferences(
                        typeof(ITag).Assembly,
                        typeof(ComChannelOption).Assembly,
                        typeof(SerialPort).Assembly
                        )
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

            var script = CSharpScript.Create<T>(
                this.ReadScript,
                scriptOptions,
                globalsType: typeof(SerialPortGlobals)
            );
            try
            {
                this._runner = script.CreateDelegate();
            }
            catch(CompilationErrorException ex)
            {
                this._logger.LogError("脚本编译错误：{exMsg}, {strace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        var str = await this._runner.Invoke(new SerialPortGlobals(serial), ct);
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


    public override void Dispose()
    {
        this.TryClearOldScriptPath();
        base.Dispose();
    }
}
