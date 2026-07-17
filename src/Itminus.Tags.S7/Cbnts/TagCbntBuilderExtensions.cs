namespace Itminus.Tags.S7;

/// <summary>
/// extensions for <see cref="TagCbntBuilderBase"/>
/// </summary>
public static class TagCbntBuilderExtensions
{
    /// <summary>
    /// 创建 <see cref="S7TagFactory"/>
    /// </summary>
    /// <param name="tagGroupBuilder"></param>
    /// <returns></returns>
    public static S7TagFactory MakeS7TagFactory(this TagCbntBuilderBase tagGroupBuilder)
    {
        var tagFactory = new S7TagFactory(tagGroupBuilder);
        return tagFactory;
    }


    
}