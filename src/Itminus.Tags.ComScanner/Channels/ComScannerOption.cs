using System.IO.Ports;

namespace Itminus.Tags.ComScanner.Channels;

public class ComScannerOption
{
    public string Port { get; set; } = "COM1";
    public int BaundRate { get; set; }
    public Parity Parity { get; set; }
    public int DataBits { get; set; }
    public StopBits StopBits { get; set; }
}
