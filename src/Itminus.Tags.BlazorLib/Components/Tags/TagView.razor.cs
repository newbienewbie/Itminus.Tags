using Itminus.Tags.BlazorLib.Components.Tags.Editing;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Reactive.Linq;
using System.Reactive.Subjects;


namespace Itminus.Tags.BlazorLib.Components.Tags;

public partial class TagView : IDisposable
{
    [Parameter]
    public ITagsProject? Project { get; set; }

    [Parameter]
    public ITag? Tag { get; set; }

    private string TagName { get; set; } = string.Empty;
    private string TagAddress { get; set; } = string.Empty;
    private TagKinds TagKind { get; set; } = BuiltinTagKinds.Unknown;
    private int TagSize { get; set; }
    public object? Value { get; set; }
    private DateTime Timestamp { get; set; }

    public bool IsReadOnly { get; private set; } = true;


    public override Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.TryGetValue<ITag>(nameof(Tag), out var tag))
        {
            _disposable?.Dispose();
            _disposable = null;

            if (tag is not null)
            {
                TagName = tag.TagName();
                TagAddress = tag.NormalizedAddress();
                TagKind = tag.TagKind();
                TagSize = tag.TagDescriptor.TagSize;
                IsReadOnly = tag.AccessMode() == TagAccessMode.RO;

                // Throttle updates and avoid re-rendering if nothing actually changed.
                _disposable = tag.Watch()
                    .TakeUntil(_destroySignal)
                    .Sample(TimeSpan.FromMilliseconds(50))
                    .Subscribe(ev =>
                    {
                        if (disposedValue)
                            return;
                        var val = ev.EventArgs.NewValue;
                        var ts = ev.EventArgs.Timestamp;

                        if (IsSame(Value, val) && ts == Timestamp)
                            return;

                        _ = InvokeAsync(() =>
                        {
                            if(this.disposedValue)
                            {
                                return;
                            }

                            Value = val;
                            Timestamp = ts;
                            StateHasChanged();
                        }).ContinueWith(t =>
                        {
                            if (t.Exception != null)
                            {
                                // 记录日志或忽略（组件已释放时的异常）
                                foreach (var ex in t.Exception.InnerExceptions)
                                {
                                    if (ex is ObjectDisposedException or InvalidOperationException)
                                    {
                                        ; // 安全忽略
                                    }
                                    else
                                    {
                                        throw ex; // 其他异常不应吞没
                                    }
                                }
                            }
                        }, TaskScheduler.Default);
                    });
            }
        }

        return base.SetParametersAsync(parameters);
    }

    private static bool IsSame(object? a, object? b)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;
        return a.Equals(b);
    }

    private async Task OpenEditDialog()
    {
        if (Tag is null)
            return;

        var parameters = new DialogParameters<TagEditDialog>
        {
            { x => x.Project, this.Project },
            { x => x.Tag, Tag },
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth = true, MaxWidth = MaxWidth.ExtraSmall };
        await DialogService.ShowAsync<TagEditDialog>("编辑测点", parameters, options);
    }

    #region IDisposable Support
    private IDisposable? _disposable;
    private bool disposedValue;
    private readonly Subject<System.Reactive.Unit> _destroySignal = new Subject<System.Reactive.Unit>();
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                try
                {
                    this._destroySignal.OnNext(System.Reactive.Unit.Default);
                }
                catch{ }

                try
                {
                    this._destroySignal.OnCompleted();
                }
                catch { }

                try
                {
                    _disposable?.Dispose();
                }
                catch { }
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}