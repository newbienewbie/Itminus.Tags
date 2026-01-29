using Itminus.Tags.Plugins;
using Itminus.Tags.Projects;
using System.Xml.Linq;

namespace Itminus.Tags.Projects;

internal class TagsProject : ITagsProject
{
    private readonly IChannelsLoader _channelsLoader;
    private readonly ITagsLoader _tagsLoader;
    private readonly ILogicetLoader _logicetLoader;
    private readonly ILogicetMaker _logicetMaker;
    private readonly IServiceProvider _sp;

    public TagsProject(IChannelsLoader channelsLoader, ITagsLoader tagsLoader, ILogicetLoader logicetLoader, ILogicetMaker logicetMaker, IServiceProvider sp)
    {
        this._channelsLoader = channelsLoader;
        this._tagsLoader = tagsLoader;
        this._logicetLoader = logicetLoader;
        this._logicetMaker = logicetMaker;
        this._sp = sp;
    }

    /// <summary>
    /// 项目更目录
    /// </summary>
    public string? ProjectRoot { get; private set; } = string.Empty;

    /// <summary>
    /// 从根元素中加载通道
    /// </summary>
    /// <param name="channelFactory"></param>
    /// <returns></returns>
    protected virtual TagsProject LoadChannels(XElement root)
    {
        var elements = root.Elements("Channel") ?? [];
        var descriptors = elements.Select(ChannelDescriptor.LoadFromXElement);
        var channels = this._channelsLoader.LoadChannels(descriptors);
        this.AddChannels(channels);
        return this;
    }

    /// <summary>
    /// 加载测点
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    protected virtual TagsProject LoadTags(XElement root)
    {
        var main = new TagGrp(name: "__main__", isEntry: false, null);
        var elements = root.Elements().Where(e => e.IsTagUnion())?? [];
        foreach (var ele in elements) 
        {
            this._tagsLoader.LoadTagGroup(main, ele, this.Channels);
        }
        this.Tags = main;
        return this;
    }


    /// <summary>
    /// 加载 Logicets
    /// </summary>
    /// <param name="loader"></param>
    /// <returns></returns>
    protected virtual TagsProject LoadLogicets(XElement root)
    {
        var elements = root.Elements("Logicet");
        var dlls = elements
            .Where(e => !string.IsNullOrEmpty( e.Value) )
            .Select(e => string.IsNullOrEmpty(this.ProjectRoot) ? e.Value : Path.Combine(this.ProjectRoot, e.Value));
        var logicets = this._logicetLoader.LoadLogicets(this._sp, dlls, this.Channels, this.Tags);
        this.AddLogicets(logicets);
        return this;
    }

    /// <inheritdoc/>
    public virtual bool TryAddLogicet<TLogicet>(out TLogicet? logicet, out string? msg)
        where TLogicet : class, ILogicet
    {
        logicet = this._logicetMaker.MakeLogicet<TLogicet>(this.Channels, this.Tags, out msg);
        if(logicet is not null)
        {
            this.Logicets.Add(logicet);
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public virtual bool TryAddLogicet<TLogicet>()
        where TLogicet : class, ILogicet
    {
        return this.TryAddLogicet<TLogicet>(out _, out _);
    }

    /// <inheritdoc/>
    public void Initialize(string projRoot, XElement? root=null)
    {
        this.ProjectRoot = projRoot;

        this._channels.Clear();
        this.Tags = null!;
        this._logicets.Clear();

        if(root is null)
        {
            var rootxmlPath = Path.Combine(projRoot, "index.xml");
            if(!File.Exists(rootxmlPath))
            {
                throw new FileNotFoundException(rootxmlPath);
            }
            root = XElement.Load(rootxmlPath);
        }
        this.LoadChannels(root);
        this.LoadTags(root);
        this.LoadLogicets(root);
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


    public virtual Task RunAsync(CancellationToken ct)
    {
        if (this.Channels == null || this.Channels.Count == 0)
        {
            throw new Exception($"通道集为空");
        }
        if (this.Tags == null)
        {
            throw new Exception("测点集为空");
        }
        if (this.Logicets == null)
        {
            throw new Exception("逻辑组件集为空");
        }

        var entries = this.Tags.ScanEntries();
        if (entries.Count == 0)
        {
            throw new Exception("未配置入口测点组");
        }


        var tasks = entries.Select(async entry => {
            var logicets = this.Logicets
                .Where(l => l.MatchEntry(entry))
                .OrderBy(l => l.Order)
                .ToList();
            var monitor = new TagGrpRunner();
            monitor.TurnStarted += TurnStarted;
            monitor.TurnProcess += async (entry, ch) => {
                foreach (var l in logicets)
                {
                    await l.ProcessAsync(entry, ch);
                }
            };
            monitor.TurnCrashed += TurnCrashed;
            await monitor.StartAsync(entry, ct);
        });
        return Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public event TurnCrashed? TurnCrashed;
    /// <inheritdoc/>
    public event TurnStarted? TurnStarted;
}
