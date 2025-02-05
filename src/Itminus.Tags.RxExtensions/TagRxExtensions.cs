using System.Reactive.Linq;
using System.Reactive;

namespace Itminus.Tags;

public static class TagRxExtensions
{

    public static IObservable<EventPattern<ITag, TagSyncEventArgs>> Watch(this ITag tag)
    {
        var obs = Observable.FromEventPattern<TagSyncEventHandler, ITag, TagSyncEventArgs>(
            h => tag.OnTagSync += h,
            h => tag.OnTagSync -= h
            );

        var ev = new EventPattern<ITag,TagSyncEventArgs>(
            tag, 
            new TagSyncEventArgs(tag.Value, tag.Timestamp)
            );
        return obs.StartWith(ev);
    }
}
