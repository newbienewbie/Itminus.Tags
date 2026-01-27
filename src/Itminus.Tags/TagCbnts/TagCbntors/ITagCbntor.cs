namespace Itminus.Tags;


/// <summary>
/// 代表属于测点组合的测点组合子，不单独使用，而是和其它组合子一起组合成一个测点组合使用，以支持整体读取、整体写入。<br/>
/// 这些被组合在一起的测点组合子，共享同一个底层缓存，在需要读取时，会被整体读取来减少IO次数；在需要写入时，既可以单独写入底层、也可以批量整体写入底层。
/// </summary>
public interface ITagCbntor: ITag
{
    /// <summary>
    /// 表示<b>【测点本身】</b>距离【测点组合起始位置】的偏移量，单位为 byte。结合<see cref="ITagExtensions.TagSize(ITag)"/>，可以确定测点在缓存中的存储区间。<br/>
    /// 注意：本属性并不代表【测点的数据本身】距离【测点组合的起始位置】的偏移量，而是代表【测点】相对于【测点组合的起始位置】的偏移量。比如，在包含跨字节位地址的情况下，一个测点可能包含了多个值。
    /// </summary>
    public int TagOffset { get; set; }


    /// <summary>
    /// 表示【测点数据】在缓存中的真正偏移位置，单位为 byte。<br/>
    /// 本属性在大部分时候和<see cref="TagOffset" />相同，但是当测点地址包含位地址且位地址跨字节时，往往会导致本属性和<see cref="TagOffset"/>不一致。<br/>
    /// 比如S7中如果起始地址是"DB100.0"，那么测点 "DB100.100.15" 对应的是<see cref="TagOffset"/>=100，而对应的<see cref="CacheOffset"/>=101 <br/>。
    /// 再比如Modbus HoldingRegister中如果起始地址是"40001"，, 每个将Tag占据2个字节，于是测点"40009.15"的<see cref="TagOffset"/>=16，而<see cref="CacheOffset"/>=17
    /// </summary>
    public int CacheOffset { get; set; }


    /// <summary>
    /// 强行通知测点已经变化
    /// </summary>
    void NotifyTagRead();


    /// <summary>
    /// 强行通知测点已经刷入底层
    /// </summary>
    void NotifyValueWritten();
}
