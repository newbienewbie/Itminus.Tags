namespace Itminus.Tags;


public enum TagAccessMode
{
    /// <summary>
    /// R & W
    /// </summary>
    RW = 0,


    /// <summary>
    /// ReadOnly
    /// </summary>
    RO = 0,


    /// <summary>
    /// ReadOnce and then WriteOnly
    /// </summary>
    R1W = 1,
}

/// <summary>
/// 测点描述
/// </summary>
public class TagDescriptor: ITagsDescriptor
{
    /// <summary>
    /// 测点名称——用于展示，在测组中具有唯一性
    /// </summary>
    public string TagName { set; get; } = null!;

    /// <summary>
    /// 测点类型，位、字节、ushort、整型、浮点等
    /// </summary>
    public TagKinds TagKind { set; get; }

    /// <summary>
    /// 本测点所占据的内存大小，以字节为单位。<br/>
    /// 通常，在连续Byte型存储中（比如SiemensPLC的DataBlock)：<br/>
    ///     - Bit 也会占据1个字节——每个字节最多可以代表8个Bits；<br/>
    ///     - Int16 会占据2个字节；<br/>
    /// 在连续WORD型存储中(比如Modbus的保持寄存器段），<br/>
    ///     - Bit会占据2个字节，每个点可以代表16个Bits<br/>
    ///     - Float会占据4个字节——用两个连续的点来表示<br/>
    /// 在Modbus中的线圈段，每个点就是一个比特，由于计算机内存的最小单位是字节，我们仍把这种情况下的测点大小记为1<br/>
    /// <br/>
    /// 尽管大部分基础类型的TagSize都可以根据数据类型直接推导出来，但是并不是所有情况都能自动计算：<br/>
    /// 1. 对于底层是连续<bold>Byte型</bold>存储，虽然对于普通的Bit、Byte、Int16等可以在编译时就知道类型大小，但是字符串型仍需要在运行时指定测点长度。<br/>
    /// 2. 对于底层是连续<bold>WORD型</bold>存储（比如Modbus的HoldingRegisters区域）, 即使是一个Bit数据点也会占据2个字节；而Float可能占据两个数据点（4个字节）<br/>
    /// </summary>
    public int TagSize { get; set; } 

    /// <summary>
    /// 测点地址
    /// </summary>
    public TagAddress Address { set; get; } = null!;

    /// <summary>
    /// 大小尾
    /// </summary>
    public EndianKinds EndianKind { set; get; } = EndianKinds.LittleEndian;

    /// <summary>
    /// 测点访问类型
    /// </summary>
    public TagAccessMode AccessMode { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Note{ set; get; }
}
