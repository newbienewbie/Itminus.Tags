using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itminus.Tags;

namespace Itminus.Tags.ModbusTcp;

public class ModbusTcpTagCbntBuilder : TagCbntBuilderBase
{
    public ModbusTcpTagCbntBuilder()
    {
    }

    public ModbusTcpTagCbntBuilder(string cbntName, string startAddress) 
    {
        this.WithName(cbntName);
        this.WithStartAddress(startAddress);
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

    public override TagCbntBuilderBase AutoResize()
    {
        var cacheSize = 0;
        foreach (var kvp in this.TagCbnt.Children)
        {
            var tag = kvp.Value;
            var occupied = tag.TagOffset + tag.TagDescriptor.TagSize;
            if (tag is BitTagCbntor bitTag)
            {
                if (tag.CacheOffset != tag.TagOffset)
                {
                    occupied = tag.CacheOffset + 1;
                }
            }
            if (occupied > cacheSize)
            {
                cacheSize = occupied;
            }
        }
        this.TagCbnt.ResizeCache(cacheSize);
        return this;
    }
}