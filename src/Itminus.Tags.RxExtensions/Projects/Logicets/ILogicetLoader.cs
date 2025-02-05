
namespace Itminus.Tags.Projects;

/// <summary>
/// Logicet 解析器
/// </summary>
public interface ILogicetLoader
{
    /// <summary>
    /// 加载 Logicet 列表
    /// </summary>
    /// <param name="indexPath"></param>
    /// <param name="channels"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    IList<ILogicet> LoadLogicets(string indexPath, IList<ITagChannel> channels, ITagGrp tags);
}