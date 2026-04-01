namespace Itminus.Tags;

/// <summary>
/// 代表一组测点群组。群组内的各个测点是松散的，可能共享通信通道，也可能不共享通信通道。<br/>
/// 由于这种性质，群组中的测点既不会被统一读，也不会被统一写，它们的读或写往往意味着多次IO交互。<br/>
/// </summary>
public class TagGrp : ITagGrp
{
    public TagGrp(string name, bool isEntry, ITagChannel? channel)
    {
        this.Name = name;
        this.Channel = channel;
        this.IsEntry = isEntry;
    }

    /// <inheritdoc/>
    public string Name { get; set; } = "";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public ITagChannel? Channel { get; set; }

    /// <inheritdoc/>
    public ITagGrp? Parent { get; set; }

    /// <inheritdoc/>
    public bool IsEntry { get; }

    #region 子节点

    /// <inheritdoc/>
    public IDictionary<string, TagUnion> Children { get; } = new Dictionary<string, TagUnion>();

    /// <inheritdoc/>
    public TagUnion this[string tagName] => Children.TryGetValue(tagName, out var tag) ?
        tag :
        throw new Exception($"TagGrp({this.Name}) has no child who's name={tagName}");

    public virtual TagUnion Descendant(string path)
    {
        var segments = path.Split('/');
        if (segments.Length == 0)
        {
            throw new Exception($"invalid tag path={path}");
        }
        TagUnion tagunion = this[segments[0]];
        for (int idx = 1; idx < segments.Length; idx++)
        {
            var segment = segments[idx];
            tagunion = tagunion[segment];
        }
        return tagunion;
    }



    public virtual ITagGrp AddTag(ITag tag)
    {
        this.Children.Add(tag.TagName(), new TagUnion.TagUnit(tag));
        return this;
    }

    /// <summary>
    /// 增加测点
    /// </summary>
    /// <param name="tagCbnt"></param>
    /// <returns></returns>
    public virtual ITagGrp AddTag(ITagCbnt tagCbnt)
    {
        tagCbnt.Parent = this;
        this.Children.Add(tagCbnt.Name, new TagUnion.TagCbnt(tagCbnt));
        return this;
    }

    /// <summary>
    /// 增加测点
    /// </summary>
    /// <param name="tagGrp"></param>
    /// <returns></returns>
    public virtual ITagGrp AddTag(ITagGrp tagGrp)
    {
        tagGrp.Parent = this;
        this.Children.Add(tagGrp.Name, new TagUnion.TagGrp(tagGrp));
        return this;
    }
    #endregion

    /// <inheritdoc/>
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc/>
    public int ScanInterval {get;set;}

    /// <inheritdoc/>
    public async Task ReadAsync(CancellationToken ct)
    {
        foreach(var kvp in Children)
        {
            var tagunion = kvp.Value;
            await tagunion.ReadAsync(ct);
        }
    }


    /// <inheritdoc/>
    public async Task WriteAsync(CancellationToken ct)
    {
        foreach (var kvp in Children)
        {
            var tagunion = kvp.Value;
            await tagunion.WriteAsync(ct);
        }
    }

    /// <inheritdoc/>
    public bool IsDirty()
    {
        foreach(var kvp in Children)
        {
            var child = kvp.Value;
            if(child.IsDirty())
            {
                return true;
            }
        }
        return false;
    }
}
