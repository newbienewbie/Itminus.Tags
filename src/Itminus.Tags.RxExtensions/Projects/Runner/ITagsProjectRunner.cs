using System.Reactive;

namespace Itminus.Tags.Projects;

public interface ITagsProjectRunner
{
    IObservable<Unit> RunAsObservable(TagsProject proj);
    Task RunAsTaskAsync(TagsProject proj, CancellationToken ct);
}