namespace Itminus.Tags;

public static class ITagCbntExtensions
{
    /// <summary>
    /// 获取子测点。如果指定的测点名不存在，则抛出异常
    /// </summary>
    /// <param name="tagcbnt"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static ITagCbntor SelectTag(this ITagCbnt tagcbnt, string path) => tagcbnt[path];


    public static ITagChannel? GetChannel(this ITagCbnt tagcbnt) => tagcbnt.Channel ?? tagcbnt.Parent?.GetRequiredChannel();

    public static ITagChannel GetRequiredChannel(this ITagCbnt tagcbnt) => tagcbnt.GetChannel() ?? throw new Exception($"Channel is not configured : TagCbnt({tagcbnt.Name})");
}