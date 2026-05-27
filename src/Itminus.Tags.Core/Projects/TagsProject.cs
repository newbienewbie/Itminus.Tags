using Itminus.Tags.Core.Projects;
using System.Collections.Concurrent;
using System.Threading.Channels;
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
    private readonly ConcurrentDictionary<ITagGrp, Channel<TagGrpWriteIntent>> _entryWriteIntentChannels = new();

    private List<IDisposable> _disposables = new List<IDisposable>();

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
        this.AddLogicets(logicets.Logicets);
        this._disposables.AddRange(logicets.Disposables);
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
        this.RefreshIntentChannels();
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
            var writeIntentChannel = this._entryWriteIntentChannels.GetOrAdd(entry, _ => this.CreateIntentChannel());
            var logicets = this.Logicets
                .Where(l => l.MatchEntry(entry))
                .OrderBy(l => l.Order)
                .ToList();
            var runner = this._tagGrpRunnerFactory.Create(this);
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

    #region Intent Mgmt
    /// <inheritdoc/>
    public bool WriteIntent(ITagGrp entry, TagGrpWriteIntent intent)
    {
        var intentChannel = GetRequiredEntryIntentChannel(entry);
        var writer = intentChannel.Writer;
        return writer.TryWrite(intent);
    }

    private Channel<TagGrpWriteIntent> GetRequiredEntryIntentChannel(ITagGrp entry)
    {
        if (!this._entryWriteIntentChannels.TryGetValue(entry, out var intentChannel))
        {
            throw new KeyNotFoundException($"未找到入口组 {entry.Name} 对应的意图通道");
        }
        return intentChannel;
    }

    protected virtual void RefreshIntentChannels()
    {
        this.CompleteIntentChannels();

        if (this.Tags is null)
        {
            return;
        }

        foreach (var entry in this.Tags.ScanEntries())
        {
            this._entryWriteIntentChannels[entry] = this.CreateIntentChannel();
        }
    }

    protected virtual Channel<TagGrpWriteIntent> CreateIntentChannel()
    {
        return Channel.CreateUnbounded<TagGrpWriteIntent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });
    }

    protected virtual void CompleteIntentChannels()
    {
        foreach (var kvp in this._entryWriteIntentChannels)
        {
            kvp.Value.Writer.TryComplete();
        }

        this._entryWriteIntentChannels.Clear();
    }

    /// <inheritdoc/>
    public ChannelReader<TagGrpWriteIntent>? GetIntentReader(ITagGrp entry)
    {
        if (!this._entryWriteIntentChannels.TryGetValue(entry, out var intentChannel))
        {
            return null;
        }
        return intentChannel.Reader;
    }
    #endregion

    /// <inheritdoc/>
    public event TurnCrashed? TurnCrashed;
    /// <inheritdoc/>
    public event TurnStarted? TurnStarted;


    #region
    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                this.CompleteIntentChannels();
                foreach (var d in this._disposables)
                {
                    try
                    {
                        d.Dispose();
                    }
                    catch {  /*  */  }
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TagsProject()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
