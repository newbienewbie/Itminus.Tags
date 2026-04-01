using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.Tags.Editing.DefaultEditors;

public sealed class TextTagValueEditor : ITagValueEditor
{
    public int Priority => 0;

    public bool CanEdit(ITag tag)
    {
        if (tag is null)
            return false;
        if (tag.TagDescriptor.AccessMode == TagAccessMode.RO)
            return false;
        return tag.TagKind() == BuiltinTagKinds.STR;
    }

    public RenderFragment Render(ITag tag) => builder =>
    {
        builder.OpenComponent(0, typeof(TextTagValueEditorView));
        builder.AddAttribute(1, "Tag", tag);
        builder.CloseComponent();
    };
}
