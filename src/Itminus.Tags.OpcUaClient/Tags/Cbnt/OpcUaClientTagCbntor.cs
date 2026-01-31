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
    private NodeId _nodeId;

    public OpcUaClientTagCbntor(TagDescriptor tagDescriptor, ITagCbnt tagCbnt, int tagOffset, int cacheOffset) 
        : base(tagDescriptor, tagCbnt, tagOffset, cacheOffset)
    {
        var address = tagDescriptor.Address;
        this._cbnt = this.TagCbnt as OpcUaClientTagCbnt
            ?? throw new InvalidOperationException("Cbnt is not an OpcUaTagCbnt");
        this._nodeId = address;
    }

    public override object? Value {
        get 
        {
            if(!this._cbnt.Bag.TryGetValue(this._nodeId, out var nodeVal))
            {
                return null;
            }
            return nodeVal.Value;
        }
        set
        {
            this._cbnt.Bag.AddOrUpdate(this._nodeId, new DataValue() { Value = value }, (nid, v) => {
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
        var nodeId = new NodeId(this.TagAddress());
        var nodeValue = cbnt.Bag[nodeId];
        var tobeWritten = new Dictionary<NodeId, DataValue>
        {
            { nodeId, nodeValue }
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
        var nodeId = new NodeId(this.TagAddress());
        var (values, errs) = await opcUaChannel.ReadAsync([nodeId], ct);

        var value = values[0];
        cbnt.Bag[nodeId] = value;

        this.NotifyTagRead();
    }
}
