namespace Itminus.Tags.OpcUaClient;

public static class TagCbntBuilderExtensions
{
    public static OpcUaClientTagFactory MakeOpcUaTagFactory(this TagCbntBuilderBase tagGroupBuilder)
    {
        var tagFactory = new OpcUaClientTagFactory(tagGroupBuilder);
        return tagFactory;
    }
}