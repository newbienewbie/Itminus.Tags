
using Itminus.Tags.BlazorLib.Components.Tags.Editing;
using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.Tags.Editors;

sealed class TextTagValueEditor : ITagValueEditor
{
    public int Priority => int.MaxValue;

    public bool CanEdit(ITag tag)
    {
        if (tag is null)
            return false;
        if (tag.TagDescriptor.AccessMode == TagAccessMode.RO)
            return false;
        return tag.TagKind() == BuiltinTagKinds.STR;
    }

    public RenderFragment Render(ITagsProject? project, ITag tag) => builder =>
    {
        builder.OpenComponent(0, typeof(TextTagValueEditorView));
        builder.AddAttribute(1, "Project", project);
        builder.AddAttribute(2, "Tag", tag);
        builder.CloseComponent();
    };
}
