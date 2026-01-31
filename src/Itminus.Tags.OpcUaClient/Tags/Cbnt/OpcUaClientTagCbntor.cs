using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.OpcUaClient;

internal class OpcUaClientTagCbntor : TagCbntor
{
    private OpcUaClientTagCbnt _cbnt;
    public NodeId NodeId { get; }

    public OpcUaClientTagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int tagOffset, int cacheOffset) 
        : base(tagDescriptor, tagCbnt, tagOffset, cacheOffset)
    {
        var address = tagDescriptor.Address;
        this._cbnt = this.TagCbnt as OpcUaClientTagCbnt
            ?? throw new InvalidOperationException("Cbnt is not an OpcUaTagCbnt");
        this.NodeId = address;
    }

    public override object? Value {
        get 
        {
            if(!this._cbnt.Bag.TryGetValue(this.NodeId, out var nodeVal))
            {
                return null;
            }
            return nodeVal.Value;
        }
        set
        {
            this._cbnt.Bag.AddOrUpdate(this.NodeId, new DataValue() { Value = value }, (nid, v) => {
                v.Value = v;
                return v;
            });
            this.MarkDirty();
        }
    }

    /// <inheritdoc />
    public override async Task WriteAsync(CancellationToken ct)
    {
        var channel = this.TagCbnt.GetRequiredChannel();
        var opcUaChannel = channel as OpcUaClientTagChannel 
            ?? throw new InvalidOperationException("Channel is not an OpcUaTagChannel");
        var cbnt = this.TagCbnt as OpcUaClientTagCbnt
            ?? throw new InvalidOperationException("Cbnt is not an OpcUaTagCbnt");
        var nodeValue = cbnt.Bag[this.NodeId];
        var tobeWritten = new Dictionary<NodeId, DataValue>
        {
            { this.NodeId, nodeValue }
        };
        await opcUaChannel.WriteAsync(tobeWritten, ct);
        this.NotifyTagWritten();
        this.IsDirty = false;
    }

    /// <inheritdoc />
    public override async Task ReadAsync(CancellationToken ct)
    {
        var channel = this.TagCbnt.GetRequiredChannel();
        var opcUaChannel = channel as OpcUaClientTagChannel
            ?? throw new InvalidOperationException("Channel is not an OpcUaTagChannel");
        var cbnt = this.TagCbnt as OpcUaClientTagCbnt
            ?? throw new InvalidOperationException("Cbnt is not an OpcUaTagCbnt");
        var (values, errs) = await opcUaChannel.ReadAsync([this.NodeId], ct);

        var value = values[0];
        cbnt.Bag[this.NodeId] = value;

        this.NotifyTagRead();
    }
}
