using Itminus.Tags.ComScanner.Channels;
using Itminus.Tags.ComScanner.Tags;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Linq;

namespace Itminus.Tags.Web.Tags;

public class TagsProjectCtrl
{
    private readonly IServiceScopeFactory _ssf;

    public TagsProjectCtrl(IServiceScopeFactory ssf)
    {
        this._ssf = ssf;
    }


    object _lock = new object();
    public ITagsProject? Project { get; private set; }
    CancellationTokenSource? _cts;


    public async Task StartPoll(string? dir, string xmlFileName)
    {
        if (this.Project != null)
        {
            throw new Exception("当前测点项目已经启动！");
        }

        var root = XDocument.Load(xmlFileName).Root;

        using var scope = this._ssf.CreateScope();
        var sp = scope.ServiceProvider;


        try
        {
            await this.DoOneByOneAsync(() =>
            {
                this._cts = new CancellationTokenSource();
                this.Project = sp.MakeProject(dir, root);
            });
            var proj = this.Project!;
            var ct = _cts!.Token;

            var observeOnComPorts = this.ObserveOnComPorts(proj.Tags);
            observeOnComPorts
                .TakeUntil(_cts.Token)
                .Subscribe(
                    ep =>
                    {
                        var tag = ep.Sender ?? throw new Exception($"COM 测点不可为空");
                        var ch = tag.GetRequiredChannel() as ComChannelBase<string> ?? throw new Exception("COM通道不可为空");
                        var chname = ch.ChannelName;
                        var code = ep.EventArgs.NewValue as string ?? "";
                        Console.WriteLine($"-------------{chname}---------------模拟处理扫描事件={code}");
                    },
                    ex =>
                    {
                        Console.WriteLine($"！！！模拟处理扫描错误={ex.Message}");
                    }
                );

            this.StartedOrStopped?.Invoke(this, new TagsProjectEventArgs(true, proj));
            await this.Project!.RunAsync(ct);
        }
        catch (Exception ex)
        {
            // todo: logging
            Console.WriteLine($"[Tags] {ex.Message}");
            throw;
        }
        finally
        {
            await this.DoOneByOneAsync(() =>
            {
                if(this.Project is not null)
                {
                    this.Project?.Dispose();
                    this.Project = null;
                }
                this._cts = null;
            });
        }
    }
    protected virtual IObservable<EventPattern<ITag, TagSyncEventArgs>> ObserveOnComPorts(ITagGrp grp)
    {
        var comtags = new List<ComReadTag<string>>();
        var visitor = new TagTraverser(tag =>
        {
            if (tag is ComReadTag<string> t)
            {
                comtags.Add(t);
            }
        });
        var union = new TagUnion.TagGrp(grp);
        union.Accept(visitor);

        return comtags
            .Select(t => t.ObserveOnRead())
            .Merge();
    }

    public async Task StopAsync()
    {
        await this.DoOneByOneAsync(async () =>
        {


            // reset proj ctrl
            var oldchannels = this.Project?.Channels;
            try
            {
                if (this._cts != null)
                {
                    this._cts.Cancel();
                }
                this.Project = null;
            }
            catch
            {
                // ignore error when cancelling
            }

            try
            {
                // disconnect from each channel
                if (oldchannels is not null)
                {
                    foreach (var ch in oldchannels)
                    {
                        try
                        {
                            if (ch is not null)
                            {
                                await ch.DisconnectAsync(CancellationToken.None);
                            }
                        }
                        catch
                        {

                        }
                    }
                }

                this.StartedOrStopped?.Invoke(this, new TagsProjectEventArgs(false, this.Project!));
            }
            catch
            {
                // ignore errors thrown by StartedOrStopped event handlers
            }
        });

    }


    public event TagsProjectStartedOrStopped? StartedOrStopped;

    #region sema helper
    private SemaphoreSlim _sema = new SemaphoreSlim(1);

    private async Task DoOneByOneAsync(Action action)
    {
        await this._sema.WaitAsync();
        try
        {
            action();
        }
        finally
        {
            this._sema.Release();
        }
    }

    private async Task DoOneByOneAsync(Func<Task> func)
    {
        await this._sema.WaitAsync();
        try
        {
            await func();
        }
        finally
        {
            this._sema.Release();
        }
    }
    #endregion
}
