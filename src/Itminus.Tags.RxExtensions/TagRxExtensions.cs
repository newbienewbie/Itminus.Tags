using System.Reactive.Linq;
using System.Reactive;

namespace Itminus.Tags;

public static class TagRxExtensions
{

    public static IObservable<EventPattern<ITag, TagSyncEventArgs>> Watch(this ITag tag)
    {
        var read = Observable.FromEventPattern<TagSyncEventHandler, ITag, TagSyncEventArgs>(
            h => tag.OnTagRead += h,
            h => tag.OnTagRead -= h
            );
        var written = Observable.FromEventPattern<TagSyncEventHandler, ITag, TagSyncEventArgs>(
            h => tag.OnTagWritten += h,
            h => tag.OnTagWritten -= h
            );
        var obs = read.Merge(written);
        var ev = new EventPattern<ITag,TagSyncEventArgs>(
            tag, 
            new TagSyncEventArgs(tag.Value, tag.Timestamp, TagSyncEventArgs.Kinds.None)
            );
        return obs;//.StartWith(ev);
    }
}
