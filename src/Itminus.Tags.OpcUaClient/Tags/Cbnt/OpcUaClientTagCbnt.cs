using Opc.Ua;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.OpcUaClient;

internal class OpcUaClientTagCbnt : ITagCbnt
{
    internal OpcUaClientTagCbnt(string name, string startAddress)
    {
        this.Name = name;
        this.StartAddress = startAddress;
    }

    /// <inheritdoc/>
    public string Name { get; set; }

    /// <inheritdoc/>
    public ITagGrp? Parent { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, ITagCbntor> Children { get; } = new Dictionary<string, ITagCbntor>();
    /// <inheritdoc/>
    public ITagCbntor this[string tagName] => this.Children.TryGetValue(tagName, out var tag) ?
        tag :
        throw new Exception($"TagCbnt({this.Name}) has no child who's name={tagName}");

    /// <inheritdoc/>
    public int ScanInterval { get; set; }
    /// <inheritdoc/>
    public bool IsEnabled { get; set; }
    /// <inheritdoc/>
    public TagAccessMode AcessMode { get; set; }
    /// <inheritdoc/>
    public bool IsScaned { get; set; }
    /// <inheritdoc/>
    public ITagChannel? Channel { get; set; }
    /// <inheritdoc/>
    public string StartAddress { get; set; }
    /// <inheritdoc/>
    public Memory<byte> Cache { get; set; } = Memory<byte>.Empty;
    /// <inheritdoc/>
    public int CacheSize => this.Bag.Count;
    /// <inheritdoc/>
    public bool IsDirty { get; set; }

    /// <inheritdoc/>
    public void ResizeCache(int cacheSize){ }

    /// <summary>
    /// OpcUA 节点值集合
    /// </summary>
    public ConcurrentDictionary<NodeId, DataValue> Bag { get; } = new ConcurrentDictionary<NodeId, DataValue>();

    /// <inheritdoc/>
    public async Task ReadAsync(CancellationToken ct)
    {
        var channel = this.Channel as OpcUaClientTagChannel;
        if(channel is null)
        {
            throw new Exception($"TagCbnt({this.Name}) 通道应为{nameof(OpcUaClientTagChannel)},实际为{channel?.GetType()}");
        }
        var nodeIds = this.Children.Keys.Select(key => new NodeId(key)).ToList() ;
        var (values, errs) = await channel.ReadAsync(nodeIds, ct);

        for(int i =0; i< nodeIds.Count; i++)
        {
            var nodeId = nodeIds[i];
            var err = errs[i];
            //todo: 检查错误

            var value = values[i];
            this.Bag[nodeId] = value;
            this.Bag.AddOrUpdate(nodeId, value, (k, v) => value);
        }

        foreach (var kv in this.Children)
        {
            var tag = kv.Value;
            tag.NotifyTagRead();
        }
    }

    /// <inheritdoc/>
    public async Task WriteAsync(CancellationToken ct)
    {
        var channel = this.Channel as OpcUaClientTagChannel;
        if (channel is null)
        {
            throw new Exception($"TagCbnt({this.Name}) 通道应为{nameof(OpcUaClientTagChannel)},实际为{channel?.GetType()}");
        }
        var toBeWritten = this.Children
            .Where(c => c.Value.IsDirty)
            .Select(child => {
                var nodeId = new NodeId(child.Key);
                return new KeyValuePair<NodeId, DataValue>(
                    nodeId,
                    this.Bag[nodeId]
                );
            })
            .ToDictionary(kvp => kvp.Key, kvp=> kvp.Value);
        
        await channel.WriteAsync(toBeWritten, ct);

        foreach (var kv in this.Children)
        {
            var tag = kv.Value;
            tag.NotifyTagWritten();
            tag.IsDirty = false;
        }
        this.IsDirty = false;
    }
}
