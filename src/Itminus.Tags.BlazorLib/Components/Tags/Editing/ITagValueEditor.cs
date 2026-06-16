using Microsoft.AspNetCore.Components;

namespace Itminus.Tags.BlazorLib.Components.Tags.Editing;

public interface ITagValueEditor
{
    int Priority { get; }

    bool CanEdit(ITag tag);

    RenderFragment Render(ITagsProject? project, ITag tag);
}
