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
        : base(new TagCbnt("unkown_s7_cbnt_name", "unknown_s7_cbnt_start_address"))
    {
    }

    public S7TagCbntBuilder(string cbntName, string startAddress)
        :base(new TagCbnt(cbntName, startAddress))
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

        // initialize str tag prefix
        foreach (var kvp in this.TagCbnt.Children)
        {
            var tag = kvp.Value;
            if (tag is S7StrTagCbntor strTag)
            {
                this.InitializeStrTag(strTag);
            }
        }

        NormalizeTagAddress();
        return this;
    }

    private void NormalizeTagAddress()
    {
        var groupAddr = S7AddressParser.Parse(this.TagCbnt.StartAddress);
        foreach (var kvp in this.TagCbnt.Children)
        {
            var tag = kvp.Value;
            var addr = S7AddressParser.Parse(tag.TagAddress());
            if(addr.BlockSpecified)
            {
                if(addr.Area != groupAddr.Area || addr.BlockNumber != groupAddr.BlockNumber)
                {
                    var tagname = tag.TagName();
                    throw new InvalidOperationException($"Tag & Cbnt start address doesn't match(Tag={tagname}, Grp={this.Name}).");
                }
            }
            else {
                // let's keep it false to indicate it was a relative address
                addr.BlockSpecified = false;
                // fill in the area and block number from group address
                addr.Area = groupAddr.Area;
                addr.BlockNumber = groupAddr.BlockNumber;
                tag.TagDescriptor.Address = addr.ToString();
            }
        }
    }

    private void InitializeStrTag(S7StrTagCbntor tag)
    {
        var prefix = this.TagCbnt.Cache.Slice(tag.CacheOffset, 2).Span;
        prefix[0] = tag.Maxlen;
        prefix[1] = 0;
    }
}

