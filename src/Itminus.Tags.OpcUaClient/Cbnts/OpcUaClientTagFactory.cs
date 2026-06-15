namespace Itminus.Tags.OpcUaClient.Cbnts;



internal class OpcUaClientTagFactory : TagCbntorFactoryBase
{
    public OpcUaClientTagFactory(TagCbntBuilderBase builder) 
        : base(builder)
    {
    }


    public override ITagCbntor CreateTag(TagDescriptor descriptor)
    {
        var tag = new OpcUaClientTagCbntor(descriptor, CbntBuilder.TagCbnt, 0, 0);
        return tag;
    }

}
