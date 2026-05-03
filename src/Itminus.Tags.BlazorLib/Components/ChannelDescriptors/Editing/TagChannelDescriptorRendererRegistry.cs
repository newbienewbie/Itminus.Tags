namespace Itminus.Tags.BlazorLib.Components.Channels.Editing;

public sealed class TagChannelDescriptorRendererRegistry
{
    private readonly List<ITagChannelDescriptorViewer> _viewers;
    private readonly List<ITagChannelDescriptorEditor> _editors;

    public TagChannelDescriptorRendererRegistry(IEnumerable<ITagChannelDescriptorViewer>? viewers = null, IEnumerable<ITagChannelDescriptorEditor>? editors = null)
    {
        viewers ??= Array.Empty<ITagChannelDescriptorViewer>();
        editors ??= Array.Empty<ITagChannelDescriptorEditor>();
        _viewers = viewers.OrderBy(v => v.Priority).ToList();
        _editors = editors.OrderBy(e => e.Priority).ToList();
    }

    public ITagChannelDescriptorViewer? ResolveViewer(TagChannelDescriptor descriptor)
        => _viewers.FirstOrDefault(v => v.CanView(descriptor));

    public ITagChannelDescriptorEditor? ResolveEditor(TagChannelDescriptor descriptor)
        => _editors.FirstOrDefault(e => e.CanEdit(descriptor));
}
