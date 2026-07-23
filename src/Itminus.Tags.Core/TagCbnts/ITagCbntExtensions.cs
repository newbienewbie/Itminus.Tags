namespace Itminus.Tags;

/// <summary>
/// extensions for <see cref="ITagCbnt"/>
/// </summary>
public static class ITagCbntExtensions
{
    /// <summary>
    /// 获取子测点。如果指定的测点名不存在，则抛出异常
    /// </summary>
    /// <param name="tagcbnt"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static ITagCbntor SelectTag(this ITagCbnt tagcbnt, string path) => tagcbnt[path];

    /// <summary>
    /// 冒泡式获取测点的通道
    /// </summary>
    /// <param name="tagcbnt"></param>
    /// <returns></returns>
    public static ITagChannel? GetChannel(this ITagCbnt tagcbnt) => tagcbnt.Channel ?? tagcbnt.Parent?.GetChannel();

    /// <summary>
    /// 冒泡式获取测点的通道。如果没有配置通道，则抛出异常
    /// </summary>
    /// <param name="tagcbnt"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static ITagChannel GetRequiredChannel(this ITagCbnt tagcbnt) => tagcbnt.GetChannel() ?? throw new Exception($"Channel is not configured : TagCbnt({tagcbnt.Name})");

    /// <summary>
    /// 获取测点读写访问模式（可能为 null，表示未配置）。
    /// </summary>
    /// <param name="tagcbnt"></param>
    /// <returns></returns>
    internal static TagAccessMode? AccessMode(this ITagCbnt tagcbnt) => tagcbnt.AcessMode;

    /// <summary>
    /// 获取解析后的访问模式。<br/>
    /// 如果自身和父级均未配置，则默认返回 <see cref="TagAccessMode.RW"/>。
    /// </summary>
    /// <param name="tagcbnt"></param>
    /// <returns></returns>
    public static TagAccessMode GetAccessMode(this ITagCbnt tagcbnt)
        => tagcbnt.AcessMode ?? tagcbnt.Parent?.AccessMode ?? TagAccessMode.RW;

    /// <summary>
    /// 只读？
    /// </summary>
    /// <param name="tagcbnt"></param>
    /// <returns></returns>
    public static bool IsReadOnly(this ITagCbnt tagcbnt) => tagcbnt.GetAccessMode() == TagAccessMode.RO;
    /// <summary>
    /// 只写？
    /// </summary>
    public static bool IsWriteOnly(this ITagCbnt tagcbnt) => tagcbnt.GetAccessMode() == TagAccessMode.WO;
}