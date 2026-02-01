using Itminus.Tags.ModbusTcp;
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
    /// 区域
    /// </summary>
    public virtual string? Area { get; protected set; }

    /// <summary>
    /// 区域起始地址
    /// </summary>
    public abstract string AreaStartAddr { get; }


    public override TagCbntBuilderBase WithXElement(XElement cbntElement)
    {
        var name = (string?)cbntElement.Attribute("name") ?? throw new ArgumentException($"Tag 未配置名称：<{cbntElement.Name.LocalName}/>");
        var area = (string?)cbntElement.Attribute("area");
        this.Area = area;
        this.WithName(name);
        this.WithStartAddress(AreaStartAddr);
        return this;
    }


    public override TagCbntBuilderBase AddTags(IList<TagDescriptor> descriptors)
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

