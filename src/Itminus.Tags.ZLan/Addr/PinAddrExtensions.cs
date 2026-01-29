using Itminus.Tags.ModbusTcp;

namespace Itminus.Tags.ZLan;

public static class PinAddrExtensions
{
    public static string ToModbusTcpAddr(this DIPinAddr pin)
    {
        var addr = ModbusTcpAddress.INPUT_CONTACTS_BASE + (ushort)pin;
        return addr.ToString();
    }

    public static string ToModbusTcpAddr(this DOPinAddr pin)
    {
        var addr = ModbusTcpAddress.OUTPUT_COILS_BASE + (ushort)pin;
        var repr=  $"{addr:d5}";
        return repr;
    }

}
