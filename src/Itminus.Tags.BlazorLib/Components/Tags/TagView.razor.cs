using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.Tags;



public partial class TagView: IDisposable
{
    [Parameter]
    public ITag? Tag { get; set; }

    private string TagName { get; set; } = string.Empty;
    private string TagAddress { get; set; } = string.Empty;
    private TagKinds TagKind { get; set; }
    private int TagSize { get; set; }
    public object? Value { get; set; }
    private DateTime Timestamp { get; set; }



    public override Task SetParametersAsync(ParameterView parameters)
    {
        if(parameters.TryGetValue<ITag>(nameof(Tag), out var tag))
        {
            this._disposable?.Dispose();
            this._disposable = null;

            if(this.Tag is not null)
            {
                this.TagName = tag.TagName();
                this.TagAddress = tag.TagAddress();
                this.TagKind = tag.TagKind();
                this.TagSize = tag.TagDescriptor.TagSize;
                this._disposable = tag.Watch()
                    .Sample(TimeSpan.FromMilliseconds(50))
                    .Subscribe(ev =>
                    {
                        var val = ev.EventArgs.NewValue;
                        var ts = ev.EventArgs.Timestamp;

                        this.Value = val;
                        this.InvokeAsync(StateHasChanged);
                    });
            }
        }

        return base.SetParametersAsync(parameters);
    }


    private bool IsSame(object? a, object? b)
    {
        if(a == null)
        {
            if ( b == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if(b != null)
        {
            return true;
        }
        else
        {
            return a.Equals(b);
        }
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
                    this._disposable?.Dispose();
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