using Itminus.FSharpExtensions;
using Microsoft.FSharp.Core;
using System.Text.RegularExpressions;

namespace Itminus.Tags.S7;


public enum AreaKinds
{ 
    None,
    MB,
    DB,
}

public struct S7Address
{

    public S7Address()
    {
    }

    public AreaKinds Area = AreaKinds.DB;

    public int BlockNumber = 0;

    /// <summary>
    /// Block已经指定
    /// </summary>
    public bool BlockSpecified = true;

    public int StartAddress = 0;

    public bool UseBit = false;

    /// <summary>
    /// 第nth位，取值范围是 0~15
    /// </summary>
    public byte NthBit = 0;

    /// <summary>
    /// 转成 TagAdress 字符串
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public override string ToString()
    {
        var str = this.Area switch
        {
            AreaKinds.MB => $"MB.{StartAddress}",
            AreaKinds.DB => $"DB{BlockNumber}.{StartAddress}",
            AreaKinds.None => $"$${StartAddress}",
            _ => throw new Exception($"未预料的S7 Area类型={this.Area}")
        };
        if (!UseBit)
        {
            return str;
        }
        return $"{str}.{NthBit}";
    }

    /// <summary>
    /// 展示格式化的地址字符串，
    /// 格式化结果与输入的地址字符串格式相同，例如：
    /// 如果输入的地址是"$$104.3"，则格式化结果也是"$$104.3";
    /// 如果输入的地址是"DB200.100"，则格式化结果也是"DB200.100"
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string Format()
    {
        var str = (this.BlockSpecified, this.Area) switch
        {
            (false, _) => $"$${StartAddress}",
            (true, AreaKinds.MB) => $"MB.{StartAddress}",
            (true, AreaKinds.DB) => $"DB{BlockNumber}.{StartAddress}",
            _ => throw new Exception($"未预料的S7 Area类型={this.Area}")
        };
        if (!UseBit)
        {
            return str;
        }
        return $"{str}.{NthBit}";
    }
}

public static class S7AddressParser
{
    public static S7Address Parse(string addr)
    {
        var addrspan = addr.AsSpan();
        if (addrspan.Length < 2)
        {
            throw new Exception($"S7地址格式错误:{addr}长度不足2");
        }

        if (addrspan[0] == '$' && addrspan[1] == '$')
        {
            return ParseRelativeAddress(addrspan[2..]);
        }

        if (addrspan.Length >= 3 && addrspan[0] == 'M' && addrspan[1] == 'B' && addrspan[2] == '.')
        {
            return ParseMBAddress(addrspan[3..]);
        }

        if (addrspan[0] == 'D' && addrspan[1] == 'B')
        {
            var q = ParseDBAddressWithNthBit(addr).OrElse(_ => ParseDBAddressWithoutNthBit(addr));
            if (q.IsError)
            {
                throw new Exception($"非法的S7DB地址({addr})格式: {q.ErrorValue}");
            }
            return q.ResultValue;
        }

        throw new Exception($"非法的S7地址格式:{addr}");
    }

    private static FSharpResult<S7Address, string> ParseDBAddressWithNthBit(string addr)
    {
        var regex = new Regex(@"DB(?<db>[0-9]+)\.(?<start>[0-9]+)\.(?<nth>[0-9]+)$");
        var match = regex.Match(addr);
        if (!match.Success)
        {
            return $"未能匹配模式 DB<db>.<start>.<nth>的模式".ToErrResult<S7Address, string>();
        }
        else
        {
            var dbStr = match.Groups["db"].Value;
            var startStr = match.Groups["start"].Value;
            var nthStr = match.Groups["nth"].Value;

            if (!int.TryParse(dbStr, out var db))
            {
                return $"无法解析db号".ToErrResult<S7Address, string>();
            }

            if (!int.TryParse(startStr, out var start))
            {
                return $"无法解析起始地址".ToErrResult<S7Address, string>();
            }

            if (!byte.TryParse(nthStr, out var nth))
            {
                return $"无法解析起始位地址".ToErrResult<S7Address, string>();
            }

            var s7Address = new S7Address
            {
                Area = AreaKinds.DB,
                BlockNumber = db,
                StartAddress = start,
                UseBit = true,
                NthBit = nth,
            };
            return s7Address.ToOkResult<S7Address, string>();
        }
    }

    private static FSharpResult<S7Address, string> ParseDBAddressWithoutNthBit(string addr)
    {
        var regex = new Regex(@"DB(?<db>[0-9]+)\.(?<start>[0-9]+)$");
        var match = regex.Match(addr);
        if (!match.Success)
        {
            return $"未能匹配模式 DB<db>.<start>的模式".ToErrResult<S7Address, string>();
        }
        var dbStr = match.Groups["db"].Value;
        var startStr = match.Groups["start"].Value;

        if (!int.TryParse(dbStr, out var db))
        {
            return $"无法解析db号".ToErrResult<S7Address, string>();
        }

        if (!int.TryParse(startStr, out var start))
        {
            return $"无法解析起始地址".ToErrResult<S7Address, string>();
        }
        var s7Address = new S7Address
        {
            Area = AreaKinds.DB,
            BlockNumber = db,
            StartAddress = start,
            UseBit = false,
            NthBit = 0,
        };
        return s7Address.ToOkResult<S7Address, string>();
    }

    /// <summary>
    /// 解析MB地址，输入类似于"2000.1"
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    private static S7Address ParseMBAddress(ReadOnlySpan<char> span)
    {
        var index = span.IndexOf('.');
        var useBit = index > 0;
        if (useBit)
        {
            var startSpan = span[..(index + 1)];
            if (!int.TryParse(startSpan, out var start))
            {
                throw new Exception($"S7地址不合法: 无法解析起始地址");
            }

            if (!byte.TryParse(span.Slice(index + 1), out var nthBit))
            {
                throw new Exception($"S7地址不合法: 无法解析位地址");
            }
            return new S7Address() 
            {
                Area =  AreaKinds.MB,
                BlockNumber = 0,
                StartAddress = start,
                UseBit = true,
                NthBit = nthBit,
            };

        }
        else 
        {
            if (!int.TryParse(span, out var start))
            { 
                throw new Exception($"S7地址不合法: 无法解析起始地址");
            }
            return new S7Address() 
            {
                Area =  AreaKinds.MB,
                BlockNumber = 0,
                StartAddress = start,
                UseBit = false,
                NthBit = 0,
            };
        }

    }

    private static S7Address ParseRelativeAddress(ReadOnlySpan<char> span)
    {
        var index = span.IndexOf('.');
        var useBit = index > 0;
        if (useBit)
        {
            var startSpan = span[..index];
            if (!int.TryParse(startSpan, out var start))
            {
                throw new Exception($"S7地址不合法: 无法解析起始地址");
            }

            if (!byte.TryParse(span.Slice(index + 1), out var nthBit))
            {
                throw new Exception($"S7地址不合法: 无法解析位地址");
            }
            return new S7Address()
            {
                Area = AreaKinds.None,
                BlockNumber = 0,
                BlockSpecified = false,
                StartAddress = start,
                UseBit = true,
                NthBit = nthBit,
            };

        }
        else
        {
            if (!int.TryParse(span, out var start))
            {
                throw new Exception($"S7地址不合法: 无法解析起始地址");
            }
            return new S7Address()
            {
                Area = AreaKinds.None,
                BlockNumber = 0,
                BlockSpecified = false,
                StartAddress = start,
                UseBit = false,
                NthBit = 0,
            };
        }

    }


}
