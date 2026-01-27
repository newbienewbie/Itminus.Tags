using System.Reactive;
using System.Reactive.Linq;
using Itminus.Tags.Projects;

namespace Itminus.Tags.RxExtensions;


public static class ITagsProjectRunnerExtensions
{

    /// <summary>
    /// 运行为一个 Observable&lt;Unit&gt;
    /// </summary>
    /// <returns></returns>
    public static IObservable<Unit> RunAsObservable(this ITagsProjectRunner runner, TagsProject proj)
    {
        return StartCancellableTask(async ct =>
        {
            await runner.RunProjAsync(proj, ct);
            return Unit.Default;
        });
    }

 

    private static IObservable<TRes> StartCancellableTask<TRes>(Func<CancellationToken, Task<TRes>> taskFunc)
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