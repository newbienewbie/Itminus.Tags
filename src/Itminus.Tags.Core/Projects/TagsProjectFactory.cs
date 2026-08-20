using Itminus.Tags.Core.Projects;
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
    private readonly IServiceProvider _sp;
    private readonly ITagsProjectSchemaValidator? _schemaValidator;

    /// <summary>
    /// c'tor
    /// </summary>
    /// <param name="grpRunnerFactory"></param>
    /// <param name="channelsLoader"></param>
    /// <param name="tagsLoader"></param>
    /// <param name="logicetLoader"></param>
    /// <param name="sp"></param>
    /// <param name="schemaValidator">
    ///     可选的加载期 schema 校验器；
    ///     未注册时为 null。
    /// </param>
    public TagsProjectFactory(
        ITagGrpRunnerFactory grpRunnerFactory,
        ITagChannelsLoader channelsLoader,
        ITagsLoader tagsLoader,
        ILogicetsLoader logicetLoader,
        IServiceProvider sp,
        ITagsProjectSchemaValidator? schemaValidator = null)
    {
        this._grpRunnerFactory = grpRunnerFactory;
        this._channelsLoader = channelsLoader;
        this._tagsLoader = tagsLoader;
        this._logicetLoader = logicetLoader;
        this._sp = sp;
        this._schemaValidator = schemaValidator;
    }

    /// <inheritdoc/>
    public virtual ITagsProject Create(string projRoot, XElement? root = null)
    {
        var project = new TagsProject(this._grpRunnerFactory, _channelsLoader, _tagsLoader, _logicetLoader, this._sp, this._schemaValidator);
        project.Initialize(projRoot, root);
        return project;
    }
}