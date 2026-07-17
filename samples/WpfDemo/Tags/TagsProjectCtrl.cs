using Itminus.Tags;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace WpfDemo.Tags;

public class TagsProjectCtrl
{
    private readonly IServiceScopeFactory _ssf;
    private readonly ILogger<TagsProjectCtrl> _logger;

    public TagsProjectCtrl(IServiceScopeFactory ssf,ILogger<TagsProjectCtrl> logger) 
    {
        this._ssf = ssf;
        this._logger = logger;
    }

    public ITagsProject? Project { get; private set; }

    CancellationTokenSource? _cts;


    public async Task StartPollAsync(string? dir, Func<ITagsProject, CancellationToken, Task> hook)
    {
        if(this.Project != null)
        {
            throw new Exception("当前测点项目已经启动！");
        }

        using var scope = this._ssf.CreateScope();
        var sp = scope.ServiceProvider;


        try
        {
            this._cts = new CancellationTokenSource();
            this.Project = sp.MakeProject(dir);
            var ct = _cts.Token;
            await hook(this.Project, ct);
            this.StartedOrStopped?.Invoke(this, new TagsProjectEventArgs(true, this.Project));
            await this.Project.RunAsync(ct);
        }
        catch(Exception ex)
        {
            this._logger.LogError(ex, "TagsProjectCtrl.StartPollAsync error");
            throw;
        }
        finally
        {
            if (this.Project is not null)
            {
                try
                {
                    this.Project.Dispose();
                }
                catch { }
                finally
                {
                    this.Project = null;
                }
            }

            this._cts = null;
        }
    }

    public async Task StopAsync()
    {
        // reset proj ctrl
        var oldchannels = this.Project?.Channels;
        try
        {
            if (this._cts != null)
            {
                this._cts.Cancel();
            }
            if (this.Project is not null)
            {
                try
                {
                    this.Project.Dispose();
                }
                catch { }
                finally
                {
                    this.Project = null;
                }
            }
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
    }

    public event TagsProjectStartedOrStopped? StartedOrStopped;
}

public class TagsProjectEventArgs: EventArgs
{
    public TagsProjectEventArgs(bool isstarted, ITagsProject project)
    {
        this.IsStarted = isstarted;
        Project = project;
    }

    public ITagsProject Project { get; }

    public bool IsStarted { get; }
}

public delegate void TagsProjectStartedOrStopped(TagsProjectCtrl sender, TagsProjectEventArgs args);