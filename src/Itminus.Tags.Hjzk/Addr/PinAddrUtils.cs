using Itminus.FSharpExtensions;
using Itminus.Tags.ModbusTcp;
using Microsoft.FSharp.Core;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Itminus.Tags.Hjzk;

public static class PinAddrUtils
{

    public static bool TryParseDI(string addr, out DIPinAddr pin)
    {
        var parsed = Enum.TryParse(addr, out pin);
        return parsed;
    }

    public static bool TryParseDO(string addr, out DOPinAddr pin)
    {
        var parsed = Enum.TryParse(addr, out pin);
        return parsed;
    }
}