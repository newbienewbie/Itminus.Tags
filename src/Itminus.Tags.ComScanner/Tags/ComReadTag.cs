using Itminus.Tags.ComScanner.Channels;
using System.IO.Ports;


namespace Itminus.Tags.ComScanner.Tags;

/// <summary>
/// 表示一个有值为T类型的COM测点。
/// 目前的设计 <br/>
/// </summary>
/// <typeparam name="T"></typeparam>
public class ComReadTag<T> : Tag<T, ComChannelBase<T>>
{
    public ComReadTag(TagDescriptor descriptor, ComChannelBase<T>? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }


    /// <summary>
    /// 通常来说，我们通过测点的getter来获取数据；
    /// 但是基本不会用测点的setter来回写<bold>状态</bold>到串口！串口型测点应该回复<bold>消息</bold>而不是写状态！<br/>
    /// 
    /// 这里的setter实现只是为了凑出“+1 -1”应该保持不变这种逻辑。外部开发者不应该使用setter<br/>
    /// </summary>
    public override T? Value
    {
        get => _value;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        [Obsolete("外部开发者不应该使用串口Tag的setter, 而是应该使用“TagGrpWriteIntent”的方式")]
        set
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
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

    /// <summary>
    /// 串口型测点的写操作通常不是直接写状态，而是回复消息，所以这里的WriteAsync不应该做任何事情。<br/>
    /// 如果有需要回复消息的场景，应该通过<see cref="TagGrpWriteIntent"/>来完成，而不是直接写测点的值。<br/>
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    [Obsolete("串口型Tag的setter/Write语义尚不清晰，外部开发不宜使用")]
    public override Task WriteAsync(CancellationToken ct)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
    {
        this.IsDirty = false;
        this.NotifyTagWritten(this._value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 当前的通道对象，用来向串口回写消息。<br/>
    /// </summary>
    public ComChannelBase<T> ComChannel => this._bubbleChannel;


    public virtual Task SendAsync(byte[] bytes)
    {
        var serialport = this.ComChannel.SerialPort;
        if(serialport is null)
        {
            throw new InvalidOperationException($"测点({this.TagName()})串口通道为null，无法发送消息！");
        }
        serialport.Write(bytes,0, bytes.Length);
        return Task.CompletedTask;
    }
}
