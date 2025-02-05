namespace Itminus.Tags.S7;

public static class TagCbntBuilderExtensions
{
    public static S7TagFactory MakeS7TagFactory(this TagCbntBuilderBase tagGroupBuilder)
    {
        var tagFactory = new S7TagFactory(tagGroupBuilder);
        return tagFactory;
    }


    
}