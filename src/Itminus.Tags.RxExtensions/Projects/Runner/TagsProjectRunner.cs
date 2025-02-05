using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Itminus.Tags.Projects;
using Microsoft.Extensions.Logging;

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

    /// <summary>
    /// 运行
    /// </summary>
    /// <returns></returns>
    public virtual IObservable<Unit> RunAsObservable(TagsProject proj)
    {
        return StartCancellableTask(async ct =>
        {
            await this.RunAsTaskAsync(proj, ct);
            return Unit.Default;
        });
    }

    /// <summary>
    /// 运行
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public virtual Task RunAsTaskAsync(TagsProject proj, CancellationToken ct)
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

        var tasks = entries.Select(e => RunCoreAsync(proj.Channels, e, proj.Logicets, ct));
        return Task.WhenAll(tasks);
    }


    /// <summary>
    /// 运行一个测点群组
    /// </summary>
    /// <param name="grp"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected virtual async Task RunCoreAsync(IList<ITagChannel> channels, ITagGrp grp, IList<ILogicet> logicets, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var disposeAll = new CompositeDisposable();
            var channel = grp.GetRequiredChannel();
            try
            {
                if (!grp.IsEnabled)
                {
                    await Task.Delay(500);
                    continue;
                }

                AttachLogicets(logicets, disposeAll);

                while (!ct.IsCancellationRequested)
                {
                    await channel.EnsureConnectedAsync();
                    await grp.ReadAsync();
                    await grp.WriteAsync();
                    await Task.Delay(grp.ScanInterval);
                }

            }
            catch (Exception ex)
            {
                disposeAll.Dispose();
                var msg = $"{ex.Message}\r\n{ex.StackTrace}";
                this._logger.LogError("通道={channel}处理报错消息出错：{ex}", channel.ChannelName, msg);
                try
                {
                    channel?.DisconnectAsync();
                }
                catch
                {

                }
            }
            finally
            {
                await Task.Delay(grp.ScanInterval, ct);
            }
        }
    }


    /// <summary>
    /// 连接处理器，会导致内部的所有 <see cref="ILogicet"/> 被挂载
    /// </summary>
    /// <param name="disposeAll"></param>
    protected virtual void AttachLogicets(IList<ILogicet> logicets, CompositeDisposable disposeAll)
    {
        foreach (var logicet in logicets)
        {
            var dispose = logicet.Attach();
            disposeAll.Add(dispose);
        }
    }


    private IObservable<TRes> StartCancellableTask<TRes>(Func<CancellationToken, Task<TRes>> taskFunc)
    {
        return Observable.Create<TRes>(async (observer, cancellationToken) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                var res = await taskFunc(cts.Token);
                observer.OnNext(res); // 通知完成
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
            return () => cts.Cancel();
        });
    }
}