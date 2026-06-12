using System;
namespace Itminus.Tags.OpcUaClient;

public class OpcUaClientTagCbntBuilder : TagCbntBuilderBase
{
    public OpcUaClientTagCbntBuilder() 
        :base(new OpcUaClientTagCbnt("unkown_opcua_cbnt_name", "unknown_opcua_cbnt_start_address"))
    {
    }

    public OpcUaClientTagCbntBuilder(string cbntName, string startAddress) 
        :base(new OpcUaClientTagCbnt(cbntName, startAddress))
    {
        this.WithName(cbntName);
        this.WithStartAddress(startAddress);
    }

    public override TagCbntBuilderBase AddTags(IList<TagDescriptor> descriptors, ITagChannel channel)
    {
        var tagFactory = this.MakeOpcUaTagFactory();
        this.Configure(builder => {
            foreach (var descriptor in descriptors)
            {
                var tag = tagFactory.CreateTag(descriptor);
                builder.AddTag(tag);
            }
        });
        return this;
    }

    public override TagCbntBuilderBase AutoResize()
    {
        return this;
    }
}