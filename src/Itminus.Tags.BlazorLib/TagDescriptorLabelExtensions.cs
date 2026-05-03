namespace Itminus.Tags.BlazorLib;

internal static class TagDescriptorLabelExtensions
{
    public static string? GetLabel(this ITagsDescriptor? d)
    {
        return d is null ? null :
             d is TagGrpDescriptor grpDescriptor ? grpDescriptor.Name :
             d is TagCbntDescriptor cbntDescriptor ? cbntDescriptor.Name :
             d is TagDescriptor descriptor ? descriptor.TagName :
             d.ToString();
    }
}
