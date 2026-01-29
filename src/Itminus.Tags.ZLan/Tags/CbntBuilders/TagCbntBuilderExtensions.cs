namespace Itminus.Tags.ZLan;

public static class TagCbntBuilderExtensions
{
    public static ZLanTagFactory MakeZLanTagFactory(this TagCbntBuilderBase tagGroupBuilder)
    {
        var tagFactory = new ZLanTagFactory(tagGroupBuilder);
        return tagFactory;
    }
}