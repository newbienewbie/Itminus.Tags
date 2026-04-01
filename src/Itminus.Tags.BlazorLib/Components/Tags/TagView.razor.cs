using Itminus.Tags.BlazorLib.Components.Tags.Editing;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Reactive.Linq;


namespace Itminus.Tags.BlazorLib.Components.Tags;

public partial class TagView : IDisposable
{
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
                TagAddress = tag.TagAddress();
                TagKind = tag.TagKind();
                TagSize = tag.TagDescriptor.TagSize;
                IsReadOnly = tag.AccessMode() == TagAccessMode.RO;

                // Throttle updates and avoid re-rendering if nothing actually changed.
                _disposable = tag.Watch()
                    .Sample(TimeSpan.FromMilliseconds(50))
                    .Subscribe(ev =>
                    {
                        var val = ev.EventArgs.NewValue;
                        var ts = ev.EventArgs.Timestamp;

                        if (IsSame(Value, val) && ts == Timestamp)
                            return;

                        Value = val;
                        Timestamp = ts;
                        _ = InvokeAsync(StateHasChanged);
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
            { x => x.Tag, Tag }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, FullWidth = true, MaxWidth = MaxWidth.ExtraSmall };
        await DialogService.ShowAsync<TagEditDialog>("编辑测点", parameters, options);
    }

    #region
    private IDisposable? _disposable;
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                try
                {
                    _disposable?.Dispose();
                }
                catch
                {
                }
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