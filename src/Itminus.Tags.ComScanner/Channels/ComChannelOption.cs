using System.IO.Ports;

namespace Itminus.Tags.ComScanner.Channels;

public class ComChannelOption
{
    /// <summary>
    /// 换行符。null表示使用系统默认的换行符。<br/>
    /// </summary>
    public string? NewLine { get; set; } 
    public string Port { get; set; } = "COM1";
    public int BaundRate { get; set; }
    public Parity Parity { get; set; }
    public int DataBits { get; set; }
    public StopBits StopBits { get; set; }

    /// <summary>
    /// 通道元素数量
    /// </summary>
    public int ChannelCapacity { get; set; } =1;

    /// <summary>
    /// 脚本
    /// </summary>
    public string? ReadScript { get; set; }

    /// <summary>
    /// 脚本调试开关，开启后会将脚本内容写入临时文件，并在编译时附加调试信息，以便在调试器中查看脚本代码
    /// </summary>
    public bool ReadScriptDebugInformationEnabled { get; set; } = false;
}
