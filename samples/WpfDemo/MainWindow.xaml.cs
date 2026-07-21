using Itminus.Tags;
using Itminus.Tags.Rx;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Windows;

namespace WpfDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private CompositeDisposable _disposables;

    public MainWindow()
    {
        InitializeComponent();
        this._disposables = new CompositeDisposable();

        var app = App.Current as App ?? throw new InvalidCastException("App.Current is not of type App");
        var tags = app.Ctrl!.Project!.Tags;
        SubscribeTags(tags);
    }

    private void SubscribeTags(ITagGrp tags)
    {
        var req = tags.SelectTag("IoBox/通用状态/PLC/心跳请求");
        var ack = tags.SelectTag("IoBox/通用状态/MST/心跳响应");
        var interval = tags.SelectTag("IoBox/通用状态/MST/扫描周期");

        req.Watch()
            .ObserveOn(DispatcherScheduler.Current)
            .Subscribe(evt =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.txtReq.Text = evt.EventArgs.NewValue?.ToString();
                });
            })
            .DisposeWith(_disposables);

        ack.Watch()
            .ObserveOn(DispatcherScheduler.Current)
            .Subscribe(evt =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.txtAck.Text = evt.EventArgs.NewValue?.ToString();
                });
            })
            .DisposeWith(_disposables);

        interval.Watch()
            // 统计连续5次的平均值
            .Window(5)
            .SelectMany(wnd =>
                wnd.Select(evt => {
                    var val = evt.EventArgs.NewValue;
                    return val is null ? 0 : (float)val;
                })
                .Average()
            )
            .ObserveOn(DispatcherScheduler.Current)
            .Subscribe(val =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.txtInterval.Text = $"{val:F3} ms";
                });
            })
            .DisposeWith(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}