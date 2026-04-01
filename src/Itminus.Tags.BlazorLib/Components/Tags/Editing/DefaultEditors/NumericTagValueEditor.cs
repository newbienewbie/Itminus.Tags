using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.Tags.Editing.DefaultEditors;

public sealed class NumericTagValueEditor : ITagValueEditor
{
    public int Priority => 50;

    public bool CanEdit(ITag tag)
    {
        if (tag is null)
            return false;
        if (tag.TagDescriptor.AccessMode == TagAccessMode.RO)
            return false;

        var kind = tag.TagKind();
        return kind == BuiltinTagKinds.BYTE
            || kind == BuiltinTagKinds.INT16
            || kind == BuiltinTagKinds.UINT16
            || kind == BuiltinTagKinds.INT32
            || kind == BuiltinTagKinds.UINT32
            || kind == BuiltinTagKinds.INT64
            || kind == BuiltinTagKinds.UINT64
            || kind == BuiltinTagKinds.FLOAT;
    }

    public RenderFragment Render(ITag tag) => builder =>
    {
        builder.OpenComponent(0, typeof(NumericTagValueEditorView));
        builder.AddAttribute(1, "Tag", tag);
        builder.CloseComponent();
    };
}
