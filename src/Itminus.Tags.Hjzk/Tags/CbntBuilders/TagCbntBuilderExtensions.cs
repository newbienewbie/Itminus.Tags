namespace Itminus.Tags.Hjzk;

public static class TagCbntBuilderExtensions
{
    public static HjzkTagFactory MakeHjzkTagFactory(this HjzkCbntBuilderBase tagGroupBuilder)
    {
        var tagFactory = new HjzkTagFactory(tagGroupBuilder);
        return tagFactory;
    }
}