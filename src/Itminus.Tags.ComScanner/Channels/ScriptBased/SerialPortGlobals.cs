using System.IO.Ports;

namespace Itminus.Tags.ComScanner.Channels;

/// <summary>
/// 串口全局变量
/// </summary>
/// <param name="serial"></param>
public sealed record SerialPortGlobals(SerialPort serial);
