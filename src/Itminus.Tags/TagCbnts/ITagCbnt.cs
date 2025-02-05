namespace Itminus.Tags;

/// <summary>
/// 代表一组支持批量读或者批量写的测点组合。
/// 测点组内的这些测点共享同一段底层缓存，同属一种读写访问方式。
/// 在自动轮询时，这些测点也有相同的采样间隔。
/// </summary>
public interface ITagCbnt
{
    /// <summary>
    /// 组合名称
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// 父组合
    /// </summary>
    ITagGrp? Parent { get; set; }

    /// <summary>
    /// 子测点集合
    /// </summary>
    IDictionary<string, ITagCbntor> Children { get; }

    /// <summary>
    /// 子测点getter。如果指定的测点名不存在，则抛出异常
    /// </summary>
    /// <param name="tagName"></param>
    /// <returns></returns>
    public ITagCbntor this[string tagName] { get; }

    /// <summary>
    /// 采样间隔
    /// </summary>
    int ScanInterval{ get; set; }

    /// <summary>
    /// 是否使能？
    /// </summary>
    bool IsEnabled{ set; get; }

    /// <summary>
    /// 访问类型
    /// </summary>
    TagAccessMode AcessMode { get; set; }

    /// <summary>
    /// 是否被扫描过
    /// </summary>
    bool IsScaned { get; set; }


    #region 底层硬件相关
    /// <summary>
    /// 测点通道
    /// </summary>
    ITagChannel? Channel { get; set; }

    /// <summary>
    /// 起始地址
    /// </summary>
    string StartAddress { get; set; }

    
    /// <summary>
    /// 从底层读取数据到缓存
    /// </summary>
    /// <returns></returns>
    abstract Task ReadAsync();

    /// <summary>
    /// 刷写缓存数据到底层
    /// </summary>
    /// <returns></returns>
    abstract Task WriteAsync();
    #endregion

    /// <summary>
    /// 底层硬件相对应的字节数组（缓存）
    /// </summary>
    Memory<byte> Cache { get; set; }


    /// <summary>
    /// Cache的大小
    /// </summary>
    int CacheSize { get; set; }

    /// <summary>
    /// 表明是否有改动
    /// </summary>
    bool IsDirty { get; set; }
}
