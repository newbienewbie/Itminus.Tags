namespace Itminus.Tags.OpcUaClient;



public class OpcUaClientTagFactory : TagCbntorFactoryBase
{
    public OpcUaClientTagFactory(TagCbntBuilderBase builder) 
        : base(builder)
    {
    }


    public override ITagCbntor CreateTag(TagDescriptor descriptor)
    {
        var tag = new OpcUaClientTagCbntor(descriptor, this.CbntBuilder.TagCbnt, 0, 0);
        return tag;
    }


    /// <summary>
    /// OpcUaClient does not use tag offsets.
    /// </summary>
    protected override int GetTagOffset(TagDescriptor tagDescriptor) => throw new NotImplementedException();
}
