using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Itminus.Tags.Projects;


/// <summary>
/// 测点项目的执行器
/// </summary>
public class TagsProjectRunner : ITagsProjectRunner
{
    protected ILogger<TagsProjectRunner> _logger;
    private readonly ITagChannelFactory _channelFactory;
    private readonly ITagsLoader _tagsLoader;
    private readonly ILogicetLoader _logicetLoader;

    public TagsProjectRunner(ITagChannelFactory channelFactory, ITagsLoader tagsParser, ILogicetLoader logicetLoader, ILogger<TagsProjectRunner> logger)
    {
        this._channelFactory = channelFactory;
        this._tagsLoader = tagsParser;
        this._logicetLoader = logicetLoader;
        this._logger = logger;
    }


    /// <inheritdoc/>
    public virtual Task RunProjAsync(TagsProject proj, CancellationToken ct)
    {
        proj.Initialize(this._channelFactory, this._tagsLoader, this._logicetLoader);
        if (proj.Channels == null || proj.Channels.Count == 0)
        {
            throw new Exception($"通道集为空");
        }
        if (proj.Tags == null)
        {
            throw new Exception("测点集为空");
        }
        if (proj.Logicets == null)
        {
            throw new Exception("逻辑组件集为空");
        }

        var entries = proj.Tags.ScanEntries();
        if (entries.Count == 0)
        {
            throw new Exception("未配置入口测点组");
        }

        var disposables = new List<IDisposable>();
        AttachLogicets(proj.Logicets, disposables.Add);

        var tasks = entries.Select(async entry => {
            var monitor = new TagGrpRunner();
            monitor.TurnCrashed += async (grp, ch, ex) => { 
                await this.NotifyTurnErrorAsync(grp, ch, ex);
                disposables.ForEach(d => d.Dispose());
            };
            await monitor.StartAsync(entry, ct);
        });
        return Task.WhenAll(tasks);
    }

    /// <summary>
    /// 通知异常发生
    /// </summary>
    /// <param name="grp"></param>
    /// <param name="channel"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    protected virtual Task NotifyTurnErrorAsync(ITagGrp grp, ITagChannel channel, Exception ex)
    {
        var channelName = channel.ChannelName;
        var msg = $"{ex.Message}\r\n{ex.StackTrace}";
        this._logger.LogError("通道={channel}处理报错消息出错：{ex}", channelName, msg);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 连接处理器，会导致内部的所有 <see cref="ILogicet"/> 被挂载
    /// </summary>
    /// <param name="disposeAll"></param>
    protected virtual void AttachLogicets(IList<ILogicet>? logicets, Action<IDisposable> handleDisposeLogicet)
    {
        if(logicets is null)
        {
            return;
        }

        foreach (var logicet in logicets)
        {
            var dispose = logicet.Attach();
            handleDisposeLogicet(dispose);
        }
    }

}