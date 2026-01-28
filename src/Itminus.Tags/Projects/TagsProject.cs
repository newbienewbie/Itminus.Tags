using Itminus.Tags.Projects;

namespace Itminus.Tags.Projects;

public class TagsProject : ITagsProject
{
    public TagsProject(string projRoot)
    {
        this.ProjectRoot = projRoot;
    }

    /// <summary>
    /// 项目更目录
    /// </summary>
    public string ProjectRoot { get; }

    protected virtual string ChannelsIndexPath => Path.Combine(ProjectRoot, "channels/index.json");
    protected virtual string TagsIndexPath => Path.Combine(ProjectRoot, "tags/index.xml");
    protected virtual string LogicetsIndexPath => Path.Combine(ProjectRoot, "logicets/index.json");


    /// <summary>
    /// 加载 Channels
    /// </summary>
    /// <param name="channelFactory"></param>
    /// <returns></returns>
    protected virtual TagsProject LoadChannels(IChannelFactory channelFactory)
    {
        var descriptors = ChannelsParser.ReadChannels(this.ChannelsIndexPath);
        var channels = descriptors.Select(d => channelFactory.Create(d)).ToList();
        this.AddChannels(channels);
        return this;
    }

    /// <summary>
    /// 加载测点
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    protected virtual TagsProject LoadTags(ITagsLoader parser)
    {
        this.Tags = parser.LoadTagRootFromIndex(TagsIndexPath, this.Channels);
        return this;
    }


    /// <summary>
    /// 加载 Logicets
    /// </summary>
    /// <param name="loader"></param>
    /// <returns></returns>
    protected virtual TagsProject LoadLogicets(ILogicetLoader loader)
    {
        var logicets = loader.LoadLogicets(this.LogicetsIndexPath, this.Channels, this.Tags);
        this.AddLogicets(logicets);
        return this;
    }


    public void Initialize(IChannelFactory channelFactory, ITagsLoader tagsParser, ILogicetLoader logicetLoader)
    {
        this._channels.Clear();
        this.Tags = null!;
        this._logicets.Clear();

        this.LoadChannels(channelFactory);
        this.LoadTags(tagsParser);
        this.LoadLogicets(logicetLoader);
    }


    ////////////////////////////////////////////////////////////

    #region Channels
    private List<ITagChannel> _channels = new List<ITagChannel>();
    /// <summary>
    /// 通道
    /// </summary>
    public virtual IList<ITagChannel> Channels => _channels;

    /// <summary>
    /// 增加通道
    /// </summary>
    /// <param name="channels"></param>
    /// <returns></returns>
    protected virtual TagsProject AddChannels(IList<ITagChannel> channels)
    {
        this._channels.AddRange(channels);
        return this;
    }
    #endregion

    #region Tags
    /// <summary>
    /// 测点
    /// </summary>
    public virtual ITagGrp Tags { get; private set; } = null!;
    #endregion

    #region logicets
    private List<ILogicet> _logicets = new List<ILogicet>();

    /// <summary>
    /// 逻辑小组件
    /// </summary>
    public IList<ILogicet> Logicets => this._logicets;

    /// <summary>
    /// 批量增加逻辑组件
    /// </summary>
    /// <param name="logicets"></param>
    /// <returns></returns>
    protected TagsProject AddLogicets(IList<ILogicet> logicets)
    {
        this._logicets.AddRange(logicets);
        return this;
    }
    #endregion
}
