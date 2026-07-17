using System;
namespace Itminus.Tags.OpcUaClient.Cbnts;

internal class OpcUaClientTagCbntBuilder : TagCbntBuilderBase
{
    /// <summary>
    /// c'tor
    /// </summary>
    public OpcUaClientTagCbntBuilder() 
        :base(new OpcUaClientTagCbnt("unkown_opcua_cbnt_name", "unknown_opcua_cbnt_start_address"))
    {
    }

    /// <summary>
    /// c'tor
    /// </summary>
    public OpcUaClientTagCbntBuilder(string cbntName, string startAddress) 
        :base(new OpcUaClientTagCbnt(cbntName, startAddress))
    {
        this.WithName(cbntName);
        this.WithStartAddress(startAddress);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override TagCbntBuilderBase AutoLayout()
    {
        return this;
    }
}