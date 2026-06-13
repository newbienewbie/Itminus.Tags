namespace Itminus.Tags.ModbusTcp;

public static class TagCbntBuilderExtensions
{
    public static ModbusTcpTagFactory MakeModbusTcpTagFactory(this TagCbntBuilderBase tagGroupBuilder)
    {
        var tagFactory = new ModbusTcpTagFactory(tagGroupBuilder);
        return tagFactory;
    }
}