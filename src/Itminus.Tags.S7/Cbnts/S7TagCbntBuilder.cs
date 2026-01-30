using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.S7;

/// <summary>
/// 针对S7的测点组合构建器
/// </summary>
public class S7TagCbntBuilder : TagCbntBuilderBase
{
    public S7TagCbntBuilder()
    {
    }

    public S7TagCbntBuilder(string cbntName, string startAddress)
    {
        this.WithName(cbntName);
        this.WithStartAddress(startAddress);
    }

    public override TagCbntBuilderBase AddTags(IList<TagDescriptor> descriptors)
    {
        var tagFactory = this.MakeS7TagFactory();
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
            if(tag is BitTagCbntor bitTag)
            {
                if(tag.CacheOffset != tag.TagOffset)
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

