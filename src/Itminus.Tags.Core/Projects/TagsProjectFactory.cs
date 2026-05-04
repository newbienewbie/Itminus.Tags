using Itminus.Tags.Core.Projects;
using Itminus.Tags.Plugins;
using System.Xml.Linq;

namespace Itminus.Tags;

/// <summary>
/// 默认的 TagsProject 工厂
/// </summary>
public class TagsProjectFactory : ITagsProjectFactory
{
    private readonly ITagGrpRunnerFactory _grpRunnerFactory;
    private readonly ITagChannelsLoader _channelsLoader;
    private readonly ITagsLoader _tagsLoader;
    private readonly ILogicetsLoader _logicetLoader;
    private readonly ILogicetMaker _logicetMaker;
    private readonly IServiceProvider _sp;

    public TagsProjectFactory(ITagGrpRunnerFactory grpRunnerFactory, ITagChannelsLoader channelsLoader, ITagsLoader tagsLoader, ILogicetsLoader logicetLoader, ILogicetMaker logicetMaker,IServiceProvider sp)
    {
        this._grpRunnerFactory = grpRunnerFactory;
        this._channelsLoader = channelsLoader;
        this._tagsLoader = tagsLoader;
        this._logicetLoader = logicetLoader;
        this._logicetMaker = logicetMaker;
        this._sp = sp;
    }

    public virtual ITagsProject Create(string projRoot, XElement? root = null)
    {
        var project = new TagsProject(this._grpRunnerFactory, _channelsLoader, _tagsLoader, _logicetLoader, this._logicetMaker, this._sp);
        project.Initialize(projRoot, root);
        return project;
    }
}