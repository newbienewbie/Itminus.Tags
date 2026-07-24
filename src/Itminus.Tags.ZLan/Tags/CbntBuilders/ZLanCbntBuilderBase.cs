using Itminus.Tags.ModbusTcp;
using Itminus.Tags.TagCbntors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Itminus.Tags.ZLan;


public abstract class ZLanCbntBuilderBase: ModbusTcpTagCbntBuilder
{
    /// <summary>
    /// 区域起始地址
    /// </summary>
    public abstract string AreaStartAddr { get; }


    public override TagCbntBuilderBase WithCbntDescriptor(TagCbntDescriptor descriptor)
    {
        base.WithCbntDescriptor(descriptor);
        this.TagCbnt.StartAddress = $"{this.Slave}~{AreaStartAddr}";
        return this;
    }


    public override TagCbntBuilderBase AddTags(IList<TagDescriptor> descriptors, ITagChannel channel)
    {
        var tagFactory = this.MakeZLanTagFactory();
        this.Configure(builder => {
            foreach (var descriptor in descriptors)
            {
                var tag = tagFactory.CreateTag(descriptor);
                builder.AddTag(tag);
            }
        });
        return this;
    }

    protected override TagCbntBuilderBase AutoLayout()
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

