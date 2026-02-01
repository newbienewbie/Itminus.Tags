using System.Xml.Linq;

namespace Itminus.Tags;

public interface ITagsProjectFactory
{
    ITagsProject Create(string projRoot, XElement? root = null);
}
