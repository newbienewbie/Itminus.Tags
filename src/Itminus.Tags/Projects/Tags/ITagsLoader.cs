
namespace Itminus.Tags.Projects;

/// <summary>
/// 测点集加载器
/// </summary>
public interface ITagsLoader
{
    /// <summary>
    /// 加载测点集
    /// </summary>
    /// <param name="indexPath"></param>
    /// <param name="channels"></param>
    /// <returns></returns>
    ITagGrp LoadTagGroups(string indexPath, IList<ITagChannel> channels);
}