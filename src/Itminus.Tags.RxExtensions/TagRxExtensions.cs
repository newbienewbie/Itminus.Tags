using System.Reactive.Linq;
using System.Reactive;

namespace Itminus.Tags;

public static class TagRxExtensions
{
    public static IObservable<EventPattern<ITag, TagSyncEventArgs>> ObserveOnRead(this ITag tag)
    {
        var read = Observable.FromEventPattern<TagSyncEventHandler, ITag, TagSyncEventArgs>(
            h => tag.OnTagRead += h,
            h => tag.OnTagRead -= h
            );
        return read;
    }

    public static IObservable<EventPattern<ITag, TagSyncEventArgs>> ObserveOnWritten(this ITag tag)
    {
        var written = Observable.FromEventPattern<TagSyncEventHandler, ITag, TagSyncEventArgs>(
            h => tag.OnTagWritten += h,
            h => tag.OnTagWritten -= h
            );
        return written;
    }


    public static IObservable<EventPattern<ITag, TagSyncEventArgs>> Watch(this ITag tag)
    {
        var read = tag.ObserveOnRead();
        var written = tag.ObserveOnWritten();
        var obs = read.Merge(written);
        var ev = new EventPattern<ITag,TagSyncEventArgs>(
            tag, 
            new TagSyncEventArgs(tag.Value, tag.Timestamp, TagSyncEventArgs.Kinds.None)
            );
        return obs.StartWith(ev);
    }
}
