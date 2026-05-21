using System.Xml.Linq;

namespace Itminus.Tags.Core.Projects;

/// <summary>
/// Logicet 解析器
/// </summary>
public interface ILogicetsLoader
{
    /// <summary>
    /// 加载 Logicet 列表
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="dlls">dll路径列表</param>
    /// <param name="channels"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    IList<ILogicet> LoadLogicets(IServiceProvider sp, IEnumerable<string> dlls, IReadOnlyList<ITagChannel> channels, ITagGrp tags);
}