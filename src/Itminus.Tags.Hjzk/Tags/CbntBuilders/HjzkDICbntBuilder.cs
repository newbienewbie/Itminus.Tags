using Itminus.Tags.ModbusTcp;

namespace Itminus.Tags.Hjzk;

public class HjzkDICbntBuilder : HjzkCbntBuilderBase
{
    /// <summary>
    /// Modbus地址解析器会自动将这里的地址-1, 这里需要人为加1
    /// </summary>
    const ushort u_DI_START_ADDRESS = 10000 + HjzkAddrDefines.BASE_DI + 1;
    static string DI_START_ADDR = $"{u_DI_START_ADDRESS:d5}";


    public HjzkDICbntBuilder():base()
    {
    }


    /// <summary>
    /// 区域
    /// </summary>
    public override string? Area { get; protected set; }

    /// <summary>
    /// 区域起始地址
    /// </summary>
    public override string AreaStartAddr => DI_START_ADDR;

}

