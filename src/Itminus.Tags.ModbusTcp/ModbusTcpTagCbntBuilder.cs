using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itminus.Tags;

namespace Itminus.Tags.ModbusTcp;

public class ModbusTcpTagCbntBuilder : TagCbntBuilderBase
{
    public ModbusTcpTagCbntBuilder(string cbntName, string startAddress) : base(cbntName, startAddress)
    {
    }

    public override TagCbntBuilderBase AddTags(IList<TagDescriptor> descriptors)
    {
        var tagFactory = this.MakeModbusTcpTagFactory();
        this.Configure(builder => {
            foreach (var descriptor in descriptors)
            {
                var tag = tagFactory.CreateTag(descriptor);
                builder.AddTag(tag);
            }
        });
        return this;
    }
}