namespace Itminus.Tags.ModbusTcp;

/// <summary>
/// extensions for <see cref="TagCbntBuilderBase"/> to create <see cref="ModbusTcpTagFactory"/>
/// </summary>
public static class TagCbntBuilderExtensions
{
    /// <summary>
    /// 创建 <see cref="ModbusTcpTagFactory"/> 实例
    /// </summary>
    /// <param name="tagGroupBuilder"></param>
    /// <returns></returns>
    public static ModbusTcpTagFactory MakeModbusTcpTagFactory(this TagCbntBuilderBase tagGroupBuilder)
    {
        var tagFactory = new ModbusTcpTagFactory(tagGroupBuilder);
        return tagFactory;
    }
}