using System.Xml.Linq;

namespace Itminus.Tags.Projects;

public interface ITagsProjectFactory
{
    ITagsProject Create(string projRoot, XElement? root = null);
}
