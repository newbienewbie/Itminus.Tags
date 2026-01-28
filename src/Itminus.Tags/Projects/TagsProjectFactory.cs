namespace Itminus.Tags.Projects;

internal class TagsProjectFactory : ITagsProjectFactory
{
    private readonly IChannelsLoader channelsLoader;
    private readonly ITagsLoader tagsLoader;
    private readonly ILogicetLoader logicetLoader;

    public TagsProjectFactory(IChannelsLoader channelsLoader, ITagsLoader tagsLoader, ILogicetLoader logicetLoader)
    {
        this.channelsLoader = channelsLoader;
        this.tagsLoader = tagsLoader;
        this.logicetLoader = logicetLoader;
    }

    public ITagsProject Create()
    {
        var project = new TagsProject(channelsLoader, tagsLoader, logicetLoader);
        return project;
    }
}