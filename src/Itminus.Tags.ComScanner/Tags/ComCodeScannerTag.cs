using Itminus.Tags.ComScanner.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.ComScanner.Tags;

public class ComCodeScannerTag : Tag<string>
{
    public ComCodeScannerTag(TagDescriptor descriptor, ComScannerChannel channel) : base(descriptor)
    {
        this._channel = channel;
    }


    #region 通道
    protected ComScannerChannel _channel;

    public override ITagChannel Channel { 
        get => this._channel;
        set {
            if(this._channel is not ComScannerChannel channel)
            {
                throw new ArgumentException($"{nameof(ComCodeScannerTag)}只接受{nameof(ComScannerChannel)}型通道");
            }
            this._channel = channel;
        }
    }
    #endregion

    public override string? Value
    {
        get => _value;
        set => throw new InvalidOperationException("扫码枪只支持读取，不可写入");
    }


    public override async Task ReadAsync(CancellationToken ct)
    {
        await this._channel.EnsureConnectedAsync(force: false, ct);

        if (!this._channel.TryDequeueInput(out var str))
        {
            return;
        }

        this._value = str;
        this.NotifyTagRead(str);
    }


    public override Task WriteAsync(CancellationToken ct)
    {
        throw new InvalidOperationException("扫码枪只能读不可写入，请不要给Value复制");
    }
}
