
using System.Xml.Linq;

namespace Itminus.Tags.Projects;

/// <summary>
/// 测点集加载器
/// </summary>
public interface ITagsLoader
{
    /// <summary>
    /// 把一个 XElement 加载为 TagGrp | TagCbnt | Tag，并添加到 parent的子元素
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="thisElement"></param>
    /// <param name="availableChannels"></param>
    void LoadTagGroup(ITagGrp parent, XElement thisElement, IList<ITagChannel> availableChannels);

    /// <summary>
    /// 从一个index文件，加载测点树。
    /// </summary>
    /// <param name="indexPath"></param>
    /// <param name="channels"></param>
    /// <returns></returns>
    ITagGrp LoadTagRootFromIndex(string indexPath, IList<ITagChannel> channels);
}