namespace Itminus.Tags;

/// <summary>
/// extensions for <see cref="TagContainer"/>
/// </summary>
public static class TagContainerExtensions
{
    /// <summary>
    /// (冒泡式)获取 <see cref="ITagChannel"/>。<br/>
    /// 如果没有找到，则抛出异常<br/>
    /// </summary>
    /// <param name="tagContainer"></param>
    /// <returns></returns>
    public static ITagChannel GetRequiredChannel(this TagContainer tagContainer) => tagContainer.Map(
        handleTagCbnt: cbnt => cbnt.GetRequiredChannel(),
        handleTagGrp: grp => grp.GetRequiredChannel()
    );



    /// <summary>
    /// 转成 <see cref="TagContainer"/>
    /// </summary>
    /// <param name="cbnt"></param>
    /// <returns></returns>
    public static TagContainer IntoTagContainer(this ITagCbnt cbnt) => TagContainer.From(cbnt);

    /// <summary>
    /// 转成 <see cref="TagContainer"/>
    /// </summary>
    /// <param name="grp"></param>
    /// <returns></returns>
    public static TagContainer IntoTagContainer(this ITagGrp grp) => TagContainer.From(grp);
}



