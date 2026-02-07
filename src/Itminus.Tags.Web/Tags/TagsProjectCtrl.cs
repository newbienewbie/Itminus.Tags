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


    public async Task StartPoll(string? dir)
    {
        if (this.Project != null)
        {
            throw new Exception("当前测点项目已经启动！");
        }

        using var scope = this._ssf.CreateScope();
        var sp = scope.ServiceProvider;

        try
        {
            lock (_lock)
            {
                this._cts = new CancellationTokenSource();
                this.Project = sp.MakeProject(dir);
            }
            var observeOnComPorts = this.ObserveOnComPorts(this.Project.Tags);
            observeOnComPorts
                .TakeUntil(_cts.Token)
                .Subscribe(
                    ep => {
                        var tag = ep.Sender ?? throw new Exception($"COM 测点不可为空");
                        var ch = tag.Channel as ComScannerChannel ?? throw new Exception("COM通道不可为空");
                        var chname = ch.ChannelName;
                        var code = ep.EventArgs.NewValue as string ?? "";
                        Console.WriteLine($"[扫码枪]({chname}): 模拟处理消息:{code}");
                    },
                    async ex => {
                        Console.WriteLine($"[扫码枪]: 模拟异常处理消息:{ex.Message}");
                    }
                );

            this.StartedOrStopped?.Invoke(this, new TagsProjectEventArgs(true, this.Project));
            await this.Project.RunAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Tag]: 发生异常:{ex.Message}");
            throw;
        }
        finally
        {
            lock (_lock)
            {
                this.Project = null;
                this._cts = null;
            }
        }
    }

    protected virtual IObservable<EventPattern<ITag, TagSyncEventArgs>> ObserveOnComPorts(ITagGrp grp)
    {
        var comtags = new List<ComCodeScannerTag>();
        var visitor = new TagTraverser(tag =>
        {
            if (tag is ComCodeScannerTag t)
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

    public void Stop()
    {
        try
        {
            lock (_lock)
            {
                this._cts?.Cancel();
                this.Project = null;
            }
            this.StartedOrStopped?.Invoke(this, new TagsProjectEventArgs(false, this.Project!));
        }
        catch
        {
        }
    }


    public event TagsProjectStartedOrStopped? StartedOrStopped;
}
