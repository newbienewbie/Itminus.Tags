using Itminus.Tags.Core.Projects;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Itminus.Tags;


/// <summary>
/// 默认的实现
/// </summary>
internal class TagsProject : ITagsProject
{
    private readonly ITagChannelsLoader _channelsLoader;
    private readonly ITagsLoader _tagsLoader;
    private readonly ILogicetsLoader _logicetLoader;
    private readonly ITagGrpRunnerFactory _tagGrpRunnerFactory;
    private readonly IServiceProvider _sp;

    public TagsProject(ITagGrpRunnerFactory tagGrpRunnerFactory, ITagChannelsLoader channelsLoader, ITagsLoader tagsLoader, ILogicetsLoader logicetLoader, IServiceProvider sp)
    {
        this._channelsLoader = channelsLoader;
        this._tagsLoader = tagsLoader;
        this._logicetLoader = logicetLoader;
        this._tagGrpRunnerFactory = tagGrpRunnerFactory;
        this._sp = sp;
    }

    /// <summary>
    /// 项目更目录
    /// </summary>
    public string? ProjectRoot { get; private set; } = string.Empty;

    /// <summary>
    /// 从根元素中加载通道
    /// </summary>
    /// <returns></returns>
    protected virtual TagsProject LoadChannels(XElement root)
    {
        var descriptors = root.GetTagProjectChannelDescriptors();
        var channels = this._channelsLoader.LoadChannels(descriptors);
        this.AddChannels(channels);
        return this;
    }

    /// <summary>
    /// 加载测点
    /// </summary>
    /// <returns></returns>
    protected virtual TagsProject LoadTags(XElement root)
    {
        var main = new TagGrp(name: "__main__", isEntry: false, null);
        var descriptors = root.GetTagProjectGrpDescriptors();
        foreach (var descriptor in descriptors) 
        {
            this._tagsLoader.LoadTagGroup(main, descriptor, this.Channels);
        }
        this.Tags = main;
        return this;
    }


    /// <summary>
    /// 加载 Logicets
    /// </summary>
    /// <returns></returns>
    protected virtual TagsProject LoadLogicets(XElement root)
    {
        var elements = root.Elements("Logicet");
        var dlls = elements
            .Where(e => !string.IsNullOrEmpty( e.Value) )
            .Select(e => string.IsNullOrEmpty(this.ProjectRoot) ? e.Value : Path.Combine(this.ProjectRoot, e.Value));
        var logicets = this._logicetLoader.LoadLogicets(this._sp, dlls, this.Channels, this.Tags);
        this.AddLogicets(logicets.AsReadOnly());
        return this;
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
    public virtual IReadOnlyList<ITagChannel> Channels => _channels;

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
    protected TagsProject AddLogicets(IReadOnlyList<ILogicet> logicets)
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

        var tasks = new ConcurrentBag<Task>();
        Parallel.ForEach(entries, entry =>
        {
            var logicets = this.Logicets
                .Where(l => l.MatchEntry(entry))
                .OrderBy(l => l.Order)
                .ToList();
            var runner = this._tagGrpRunnerFactory.Create();
            runner.TurnStarted += TurnStarted;
            runner.TurnProcess += async (entry, ch) => {
                foreach (var l in logicets)
                {
                    if(!l.Enabled)
                    {
                        continue;
                    }
                    await l.ProcessAsync(entry, ch);
                }
            };
            runner.TurnCrashed += TurnCrashed;
            Task task = runner.StartAsync(entry, ct);
            tasks.Add(task);
        });

        return Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public event TurnCrashed? TurnCrashed;
    /// <inheritdoc/>
    public event TurnStarted? TurnStarted;
}
