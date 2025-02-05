namespace Itminus.Tags.ZLan;

public static class TagCombinationBuilderExtensions
{
    public static ZLanTagFactory MakeZLanTagFactory(this TagCbntBuilderBase tagGroupBuilder)
    {
        var tagFactory = new ZLanTagFactory(tagGroupBuilder);
        return tagFactory;
    }
}