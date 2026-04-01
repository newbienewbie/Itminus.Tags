namespace Itminus.Tags.BlazorLib.Components.Tags.Editing;

internal class TagValueEditorRegistry
{
    private readonly List<ITagValueEditor> _editors;

    public TagValueEditorRegistry(IEnumerable<ITagValueEditor> editors)
    {
        _editors = editors?.OrderBy(e => e.Priority).ToList() ?? new();
    }

    public ITagValueEditor? Resolve(ITag tag)
    {
        return _editors.FirstOrDefault(e => e.CanEdit(tag));
    }
}
