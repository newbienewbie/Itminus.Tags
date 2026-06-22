using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Itminus.Tags.ComScanner.Channels;


public class ComChannelDescriptor : TagChannelDescriptor
{

    public ComChannelOption Option { get;set;} = new ComChannelOption();


    public override XElement ToXElement()
    {
        var ele = base.ToXElement();
        if(!string.IsNullOrEmpty(this.Option.NewLine))
        {
            ele.SetOrAddChild(nameof(Option.NewLine), this.Option.NewLine);
        }
        if(!string.IsNullOrEmpty(this.Option.ReadScript))
        {
            ele.SetOrAddChild(nameof(Option.ReadScript), this.Option.ReadScript);
        }
        if(this.Option.ReadScriptDebugInformationEnabled)
        {
            ele.SetOrAddChild(nameof(Option.ReadScriptDebugInformationEnabled), this.Option.ReadScriptDebugInformationEnabled);
        }
        ele.SetOrAddChild(nameof(Option.Port), this.Option.Port);
        ele.SetOrAddChild(nameof(Option.BaundRate), this.Option.BaundRate);
        ele.SetOrAddChild(nameof(Option.Parity), this.Option.Parity);
        ele.SetOrAddChild(nameof(Option.DataBits), this.Option.DataBits);
        ele.SetOrAddChild(nameof(Option.StopBits), this.Option.StopBits);
        ele.SetOrAddChild(nameof(Option.ChannelCapacity), this.Option.ChannelCapacity);
        return ele;
    }
}

public static class TagChannelDescriptor_ComExtensions
{
    public static ComChannelDescriptor ToComChannelDescriptor(this TagChannelDescriptor descriptor)
    {
        if (descriptor.Driver != ComDriverNames.DriverName)
        {
            throw new InvalidOperationException($"通道驱动错误：期望 {ComDriverNames.DriverName}，而当前为{descriptor.Driver}");
        }
        if (descriptor is ComChannelDescriptor d)
        {
            return d;
        }

        int defaultBaundRate = 9600;
        Parity defaultParity = Parity.None;
        int defaultDataBits = 8;
        StopBits defaultStopBits = StopBits.None;
        int defaultChannelCapacity = 1;

        string? newline = null;
        if (descriptor.Extras.TryGetValue(nameof(ComChannelDescriptor.Option.NewLine), out var newLine))
        {
            var raw = newLine.Value ?? string.Empty;
            // If XML contains literal escape sequences like "\\r\\n", unescape them to actual control chars
            if (raw.Contains("\\r") || raw.Contains("\\n") || raw.Contains("\\t"))
            {
                raw = Regex.Unescape(raw);
            }
            newline = raw;
        }

        var readscript = !descriptor.Extras.TryGetValue(nameof(ComChannelDescriptor.Option.ReadScript), out var readScript) ?
                 null :
                 readScript.Value;
        var readScriptDebugInformationEnabled = 
                 !descriptor.Extras.TryGetValue(nameof(ComChannelDescriptor.Option.ReadScriptDebugInformationEnabled), out var readScriptDebugInformationEnabledStr) ?false :
                 bool.TryParse(readScriptDebugInformationEnabledStr.Value, out var readScriptDebugInformationEnabledVal) ? readScriptDebugInformationEnabledVal :
                 throw new Exception($"串口读取脚本调试信息开关非法，无法解析成布尔值({readScriptDebugInformationEnabledStr.Value})");

        var port = !descriptor.Extras.TryGetValue(nameof(ComChannelDescriptor.Option.Port), out var comPort) ?
                    "COM1" :
                    comPort.Value;
        var baundRate =
                    !descriptor.Extras.TryGetValue(nameof(ComChannelDescriptor.Option.BaundRate), out var baundRateStr) ? defaultBaundRate :
                    int.TryParse(baundRateStr.Value, out var baundRateVal) ? baundRateVal :
                    throw new Exception($"串口波特率非法，无法解析成整数({baundRateStr.Value})");
        var parity = !descriptor.Extras.TryGetValue(nameof(ComChannelDescriptor.Option.Parity), out var parityStr) ? defaultParity :
                    Enum.TryParse<Parity>(parityStr.Value, out var parityVal) ? parityVal :
                    throw new Exception($"串口极性非法，无法解析成Parity({parityStr.Value})");
        var databits =
                    !descriptor.Extras.TryGetValue(nameof(ComChannelDescriptor.Option.DataBits), out var databitsStr) ? defaultDataBits :
                    int.TryParse(databitsStr.Value, out var databitsVal) ? databitsVal :
                    throw new Exception($"串口数据位非法，无法解析成整数({databitsStr.Value})");
        var stopbits = !descriptor.Extras.TryGetValue(nameof(ComChannelDescriptor.Option.StopBits), out var stopbitsStr) ? defaultStopBits :
                    Enum.TryParse<StopBits>(stopbitsStr.Value, out var stopbitsVal) ? stopbitsVal :
                    throw new Exception($"串口停止位非法，无法解析成StopBits({stopbitsStr.Value})");

        var channelCapacity = !descriptor.Extras.TryGetValue(nameof(ComChannelDescriptor.Option.ChannelCapacity), out var channelCapacityStr) ? defaultChannelCapacity :
                   int.TryParse(channelCapacityStr.Value, out var channelCapacityVal) ? channelCapacityVal :
                    throw new Exception($"通道容量非法，无法解析成正整数({channelCapacityStr.Value})");

        var res = new ComChannelDescriptor
        {
            Name = descriptor.Name,
            Driver = descriptor.Driver,
            Extras = descriptor.Extras,
            Option = new ComChannelOption {
                NewLine = newline,
                ReadScript = readscript,
                ReadScriptDebugInformationEnabled = readScriptDebugInformationEnabled,
                Port = port,
                BaundRate = baundRate,
                Parity = parity,
                DataBits = databits,
                StopBits = stopbits,
                ChannelCapacity = channelCapacity,
            },
        };
        return res;
    }
}