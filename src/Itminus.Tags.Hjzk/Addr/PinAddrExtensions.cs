namespace Itminus.Tags.Hjzk;

public static class PinAddrExtensions
{
    public static string ToModbusTcpAddr(this DIPinAddr pin, byte slave)
    {
        var addr = HjzkAddrDefines.BASE_DI + (ushort)pin;
        var repr = $"{slave}~1{addr:d4}";
        return repr;
    }

    public static string ToModbusTcpAddr(this DOPinAddr pin, byte slave)
    {
        var addr = HjzkAddrDefines.BASE_DO + (ushort)pin;
        var repr=  $"{slave}~0{addr:d4}";
        return repr;
    }

}
