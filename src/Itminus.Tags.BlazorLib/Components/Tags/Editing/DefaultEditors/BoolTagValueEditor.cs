using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.Tags.Editing.DefaultEditors;

public sealed class BoolTagValueEditor : ITagValueEditor
{
    public int Priority => 100;

    public bool CanEdit(ITag tag)
    {
        if (tag is null) 
            return false;
        if (tag.TagDescriptor.AccessMode == TagAccessMode.RO) 
            return false;

        return tag.Value is bool 
            || tag.TagKind() == BuiltinTagKinds.BIT
            || tag.TagKind() == BuiltinTagKinds.DI 
            || tag.TagKind() == BuiltinTagKinds.DO;
    }

    public RenderFragment Render(ITag tag) => builder =>
    {
        builder.OpenComponent(0, typeof(BoolTagValueEditorView));
        builder.AddAttribute(1, "Tag", tag);
        builder.CloseComponent();
    };
}
