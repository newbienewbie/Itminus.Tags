using Itminus.Tags;
using Itminus.Tags.R3;
using R3;
using System.Windows;

namespace WpfDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private IDisposable _disposables;

    public MainWindow()
    {
        InitializeComponent();

        var app = App.Current as App ?? throw new InvalidCastException("App.Current is not of type App");
        var tags = app.Ctrl!.Project!.Tags;
        this._disposables = SubscribeTags(tags);
    }

    private IDisposable SubscribeTags(ITagGrp tags)
    {
        var req = tags.SelectTag("IoBox/通用状态/PLC/心跳请求");
        var ack = tags.SelectTag("IoBox/通用状态/MST/心跳响应");
        var interval = tags.SelectTag("IoBox/通用状态/MST/扫描周期");

        var d = Disposable.CreateBuilder();
        req.Watch()
            .ObserveOnCurrentDispatcher()
            .Subscribe(evt =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.txtReq.Text = evt.NewValue?.ToString();
                });
            })
            .AddTo(ref d);

        ack.Watch()
            .ObserveOnCurrentDispatcher()
            .Subscribe(evt =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.txtAck.Text = evt.NewValue?.ToString();
                });
            })
            .AddTo(ref d);

        interval.Watch()
            .Chunk(5)
            .Select(wnd =>
                wnd.Select(evt => {
                    var val = evt.NewValue;
                    return val is null ? 0 : (float)val;
                })
                .Average()
            )
            .ObserveOnCurrentDispatcher()
            .Subscribe(val =>
            {
                this.txtInterval.Text = $"{val:F3} ms";
            })
            .AddTo(ref d);
        return d.Build();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}