namespace Itminus.Tags;

/// <summary>
/// 同步事件参数
/// </summary>
public class TagSyncEventArgs : EventArgs 
{
    /// <summary>
    /// 同步类型
    /// </summary>
    public enum Kinds 
    {
        /// <summary>
        /// 空
        /// </summary>
        None    = 0,
        /// <summary>
        /// 读取
        /// </summary>
        Read    = 1,
        /// <summary>
        /// 写入
        /// </summary>
        Written = 2,
    }

    /// <summary>
    /// c'tor
    /// </summary>
    /// <param name="newValue"></param>
    /// <param name="timestamp"></param>
    /// <param name="kind"></param>
    public TagSyncEventArgs(object? newValue, DateTime timestamp, Kinds kind)
    {
        this.NewValue = newValue;
        this.Timestamp = timestamp;
        Kind = kind;
    }

    /// <summary>
    /// 新值
    /// </summary>
    public object? NewValue { get; set; }
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
    /// <summary>
    /// 种类
    /// </summary>
    public Kinds Kind { get; set; }
}


/// <summary>
/// 测点同步事件委托
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
public delegate void TagSyncEventHandler(ITag sender, TagSyncEventArgs e);

/// <summary>
/// 测点接口
/// </summary>
public interface ITag 
{
    /// <summary>
    /// 测点描述
    /// </summary>
    public TagDescriptor TagDescriptor{ get; set; }

    /// <summary>
    /// 测点值 
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp{ get; set; }

    /// <summary>
    /// 在每次从底层读取后，触发事件
    /// </summary>
    public event TagSyncEventHandler OnTagRead;

    /// <summary>
    /// 在每次向底层写入后，触发事件
    /// </summary>
    public event TagSyncEventHandler OnTagWritten;

    /// <summary>
    /// 是否被扫描过
    /// </summary>
    public bool IsScaned { get; set; }

    /// <summary>
    /// 标识数据是否发生变化，如果发生变化，会在输出阶段刷写到硬件底层
    /// </summary>
    public bool IsDirty { get; set; }

    /// <summary>
    /// 测点通道<br/>
    /// 本属性可能为null，如果需要冒泡式获取，可以使用 .GetRequiredChannel() 扩展方法。<br/>
    /// </summary>
    public ITagChannel? Channel { get; }

    /// <summary>
    /// 父容器
    /// </summary>
    public TagContainer? Parent { get; set; }

    /// <summary>
    /// 从底层中读取测点值
    /// </summary>
    /// <returns></returns>
    public Task ReadAsync(CancellationToken ct);

    /// <summary>
    /// 把当前测点值刷到底层
    /// </summary>
    /// <returns></returns>
    public Task WriteAsync(CancellationToken ct);
}

/// <summary>
/// 测点扩展方法
/// </summary>
public static class ITagExtensions
{
    /// <summary>
    /// 测点名称
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static string TagName(this ITag tag) => tag.TagDescriptor.TagName;

    /// <summary>
    /// 规范化后的测点地址
    /// </summary>
    public static TagAddress NormalizedAddress(this ITag tag) => tag.TagDescriptor.NormalizedAddress;

    /// <summary>
    /// 配置的原始测点地址
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static TagAddress RawAddress(this ITag tag) => tag.TagDescriptor.RawAddress;

    /// <summary>
    /// 所占据的字节多少
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static int TagSize(this ITag tag) => tag.TagDescriptor.TagSize;

    /// <summary>
    /// 测点种类
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static TagKinds TagKind(this ITag tag) => tag.TagDescriptor.TagKind;

    /// <summary>
    /// 大小端
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static EndianKinds TagEndian(this ITag tag) => tag.TagDescriptor.EndianKind;

    /// <summary>
    /// 访问模式
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static TagAccessMode AccessMode(this ITag tag) => tag.TagDescriptor.AccessMode;

    /// <summary>
    /// 只读？
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static bool IsReadOnly(this ITag tag) => tag.AccessMode() == TagAccessMode.RO;

    /// <summary>
    /// 只写？
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static bool IsWriteOnly(this ITag tag) => tag.AccessMode() == TagAccessMode.WO;

    /// <summary>
    /// 把当前测点转成具体类型
    /// </summary>
    /// <typeparam name="TTag"></typeparam>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static TTag AsTag<TTag>(this ITag tag) where TTag : ITag
    {
        if (tag is not TTag t)
        {
            throw new Exception($"{tag.GetType()} is not {typeof(TTag)}");
        }
        return t;
    }


    /// <summary>
    /// 获取当前测点的值
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static TValue? GetTagValue<TValue>(this ITag tag)
    {
        TValue value = (TValue)tag.Value!;
        return value;
    }

    /// <summary>
    /// (冒泡式)获取测点通道。<br/>
    /// 如果没有找到，则抛出异常。<br/>
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static ITagChannel GetRequiredChannel(this ITag tag) =>
        tag.Channel ??
        tag.Parent?.GetRequiredChannel() ?? 
        throw new Exception($"相关测点未配置通道 : Tag({tag.TagName()})");

    /// <summary>
    /// 向上冒泡检索入口
    /// </summary>
    /// <param name="tag"></param>
    /// <returns>null代表未找到入口</returns>
    public static ITagGrp? SearchEntry(this ITag tag)
    {
        var container = tag.Parent;
        return container?.Map(
            cbnt => cbnt.Parent is null ? null : GetEntryForGrp(cbnt.Parent),
            grp => GetEntryForGrp(grp)
            );

        ITagGrp? GetEntryForGrp(ITagGrp grp)
        {
            if(grp.IsEntry)
            {
                return grp;
            }
            if(grp.Parent is null)
            {
                return null;
            }
            return GetEntryForGrp(grp.Parent);
        }
    }


}