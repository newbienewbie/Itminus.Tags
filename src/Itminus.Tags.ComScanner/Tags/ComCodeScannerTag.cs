using Itminus.Tags.ComScanner.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.ComScanner.Tags;

public class ComCodeScannerTag : Tag<string, ComScannerChannel>
{
    public ComCodeScannerTag(TagDescriptor descriptor, ComScannerChannel? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }



    public override string? Value
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


    public override Task WriteAsync(CancellationToken ct)
    {
        var val = this._value;
        if(val is not null)
        {
            this._bubbleChannel.Write(val);
        }
        this.NotifyTagWritten(val);
        this.IsDirty = false;
        return Task.CompletedTask;
    }
}
