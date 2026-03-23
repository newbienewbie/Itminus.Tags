using System.IO.Ports;

namespace Itminus.Tags.ComScanner.Channels;

public class ComScannerOption
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
}
