using Itminus.Tags.ComScanner.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itminus.Tags.ComScanner.Tags;

/// <summary>
/// 抽象基类，表示一个COM测点。<br/>
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ComTagBase<T> : Tag<T, ComChannelBase<T>>
{
    public ComTagBase(TagDescriptor descriptor, ComChannelBase<T>? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }



    public override T? Value
    {
        get => _value;
        set
        {
            this._value = value;
            this.IsDirty = true;
        }
    }


    public override async Task ReadAsync(CancellationToken ct)
    {
        await this._bubbleChannel.EnsureConnectedAsync(force: false, ct);

        if (!this._bubbleChannel.TryDequeueInput(out var str))
        {
            return;
        }

        this._value = str;
        this.Timestamp = DateTime.Now;
        this.NotifyTagRead(str);
    }


    public override async Task WriteAsync(CancellationToken ct)
    {
        var val = this._value;
        if (val is not null)
        {
            var bytes = ConvertValueToBytes(val);
            await this._bubbleChannel.WriteAsync(bytes,0, bytes.Length);
        }
        this.NotifyTagWritten(val);
        this.IsDirty = false;
    }

    /// <summary>
    /// 把TValue转成字节数组，以便写入COM通道。<br/>
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    protected abstract byte[] ConvertValueToBytes(T val);

}
