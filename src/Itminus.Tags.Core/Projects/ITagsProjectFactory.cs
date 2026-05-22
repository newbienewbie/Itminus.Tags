using System.Xml.Linq;

namespace Itminus.Tags;

public interface ITagsProjectFactory
{
    /// <summary>
    /// 创建一个新的ITagsProject实例。<br/>
    /// </summary>
    /// <param name="projRoot">项目根目录</param>
    /// <param name="root">根元素。如果为空，则默认取项目根目录下的index.xml文件</param>
    /// <returns></returns>
    ITagsProject Create(string projRoot, XElement? root = null);
}
