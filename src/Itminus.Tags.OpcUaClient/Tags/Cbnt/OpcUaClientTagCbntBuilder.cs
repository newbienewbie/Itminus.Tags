using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itminus.Tags;

namespace Itminus.Tags.OpcUaClient;

public class OpcUaClientTagCbntBuilder : TagCbntBuilderBase
{
    public OpcUaClientTagCbntBuilder()
    {
        this.TagCbnt = new OpcUaClientTagCbnt(this.Name, this.StartAddress);
    }

    public OpcUaClientTagCbntBuilder(string cbntName, string startAddress) 
    {
        this.WithName(cbntName);
        this.WithStartAddress(startAddress);
    }

    public override TagCbntBuilderBase AddTags(IList<TagDescriptor> descriptors)
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