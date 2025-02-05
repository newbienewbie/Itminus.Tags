using Itminus.Tags.ModbusTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.ZLan;





public class ZLanDICbntBuilder : ModbusTcpTagCbntBuilder
{
    const ushort u_START_ADDRESS = ModbusTcpAddress.INPUT_CONTACTS_BASE + (ushort)DIPinAddr.DI1;
    public static string StartAddress = $"{u_START_ADDRESS:d5}";
    public ZLanDICbntBuilder(string cbntName): base(cbntName, StartAddress)
    {
    }

}

public class ZLanDOCbntBuilder : ModbusTcpTagCbntBuilder
{
    const ushort u_START_ADDRESS = ModbusTcpAddress.OUTPUT_COILS_BASE + (ushort)DOPinAddr.DO1;
    public static string StartAddress = $"{u_START_ADDRESS:d5}";
    public ZLanDOCbntBuilder(string cbntName) : base(cbntName, StartAddress)
    {
    }
}



