using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Itminus.Tags.ComScanner.Channels;


public class ComScannerTagChannelDescriptor : TagChannelDescriptor
{

    public ComScannerOption Option { get;set;} = new ComScannerOption();


    public override XElement ToXElement()
    {
        var ele = base.ToXElement();
        if(!string.IsNullOrEmpty(this.Option.NewLine))
        {
            ele.SetOrAddChild(nameof(Option.NewLine), this.Option.NewLine);
        }
        ele.SetOrAddChild(nameof(Option.Port), this.Option.Port);
        ele.SetOrAddChild(nameof(Option.BaundRate), this.Option.BaundRate);
        ele.SetOrAddChild(nameof(Option.Parity), this.Option.Parity);
        ele.SetOrAddChild(nameof(Option.DataBits), this.Option.DataBits);
        ele.SetOrAddChild(nameof(Option.StopBits), this.Option.StopBits);
        return ele;
    }
}

public static class TagChannelDescriptor_ComScannerExtensions
{
    public static ComScannerTagChannelDescriptor ToComScannerTagChannelDescriptor(this TagChannelDescriptor descriptor)
    {
        if (descriptor.Driver != ComScannerNames.DriverName)
        {
            throw new InvalidOperationException($"通道驱动错误：期望 {ComScannerNames.DriverName}，而当前为{descriptor.Driver}");
        }
        if (descriptor is ComScannerTagChannelDescriptor d)
        {
            return d;
        }

        int defaultBaundRate = 9600;
        Parity defaultParity = Parity.None;
        int defaultDataBits = 8;
        StopBits defaultStopBits = StopBits.None;

        string? newline = null;
        if (descriptor.Extras.TryGetValue(nameof(ComScannerTagChannelDescriptor.Option.NewLine), out var newLine))
        {
            var raw = newLine.Value ?? string.Empty;
            // If XML contains literal escape sequences like "\\r\\n", unescape them to actual control chars
            if (raw.Contains("\\r") || raw.Contains("\\n") || raw.Contains("\\t"))
            {
                raw = Regex.Unescape(raw);
            }
            newline = raw;
        }

        var port = !descriptor.Extras.TryGetValue(nameof(ComScannerTagChannelDescriptor.Option.Port), out var comPort) ?
                    "COM1" :
                    comPort.Value;
        var baundRate =
                    !descriptor.Extras.TryGetValue(nameof(ComScannerTagChannelDescriptor.Option.BaundRate), out var baundRateStr) ? defaultBaundRate :
                    int.TryParse(baundRateStr.Value, out var baundRateVal) ? baundRateVal :
                    throw new Exception($"串口波特率非法，无法解析成整数({baundRateStr.Value})");
        var parity = !descriptor.Extras.TryGetValue(nameof(ComScannerTagChannelDescriptor.Option.Parity), out var parityStr) ? defaultParity :
                    Enum.TryParse<Parity>(parityStr.Value, out var parityVal) ? parityVal :
                    throw new Exception($"串口极性非法，无法解析成Parity({parityStr.Value})");
        var databits =
                    !descriptor.Extras.TryGetValue(nameof(ComScannerTagChannelDescriptor.Option.DataBits), out var databitsStr) ? defaultDataBits :
                    int.TryParse(databitsStr.Value, out var databitsVal) ? databitsVal :
                    throw new Exception($"串口数据位非法，无法解析成整数({databitsStr.Value})");
        var stopbits = !descriptor.Extras.TryGetValue(nameof(ComScannerTagChannelDescriptor.Option.StopBits), out var stopbitsStr) ? defaultStopBits :
                    Enum.TryParse<StopBits>(stopbitsStr.Value, out var stopbitsVal) ? stopbitsVal :
                    throw new Exception($"串口停止位非法，无法解析成StopBits({stopbitsStr.Value})");

        var res = new ComScannerTagChannelDescriptor
        {
            Name = descriptor.Name,
            Driver = descriptor.Driver,
            Extras = descriptor.Extras,
            Option = new ComScannerOption {
                NewLine = newline,
                Port = port,
                BaundRate = baundRate,
                Parity = parity,
                DataBits = databits,
                StopBits = stopbits,
            },
        };
        return res;
    }
}