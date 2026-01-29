using Itminus.Tags.Plugins;
using System.Xml.Linq;

namespace Itminus.Tags.Projects;

internal class TagsProjectFactory : ITagsProjectFactory
{
    private readonly IChannelsLoader channelsLoader;
    private readonly ITagsLoader tagsLoader;
    private readonly ILogicetLoader logicetLoader;
    private readonly ILogicetMaker _logicetMaker;

    public TagsProjectFactory(IChannelsLoader channelsLoader, ITagsLoader tagsLoader, ILogicetLoader logicetLoader, ILogicetMaker logicetMaker)
    {
        this.channelsLoader = channelsLoader;
        this.tagsLoader = tagsLoader;
        this.logicetLoader = logicetLoader;
        this._logicetMaker = logicetMaker;
    }

    public ITagsProject Create(string projRoot, XElement? root = null)
    {
        var project = new TagsProject(channelsLoader, tagsLoader, logicetLoader, this._logicetMaker);
        project.Initialize(projRoot, root);
        return project;
    }
}