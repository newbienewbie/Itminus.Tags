using System;
namespace Itminus.Tags.OpcUaClient.Cbnts;

internal class OpcUaClientTagCbntBuilder : TagCbntBuilderBase
{
    /// <summary>
    /// c'tor<br/>
    /// 需要额外使用 <c>WithCbntDescriptor()</c> 设置实际描述符。
    /// </summary>
    public OpcUaClientTagCbntBuilder() 
        :base(new OpcUaClientTagCbnt(new TagCbntDescriptor { Name = "unkown_opcua_cbnt_name", StartAddress = "unknown_opcua_cbnt_start_address" }))
    {
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