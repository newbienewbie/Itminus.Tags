namespace Itminus.Tags;

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
}
