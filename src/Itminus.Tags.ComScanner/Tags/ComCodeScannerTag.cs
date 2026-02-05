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


    public override Task ReadAsync(CancellationToken ct)
    {
        var str = this._channel.ReadString();
        this.Value = str;
        return Task.CompletedTask;
    }


    public override Task WriteAsync(CancellationToken ct)
    {
        throw new InvalidOperationException($"扫码枪只能读不可写入");
    }
}
