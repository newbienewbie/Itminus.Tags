using Itminus.Tags.TagCbntors;

namespace Itminus.Tags.S7;

/// <summary>
/// 针对S7的测点组合构建器
/// </summary>
public class S7TagCbntBuilder : TagCbntBuilderBase
{
    /// <summary>
    /// c'tor<br/>
    /// 需要额外使用 <c>WithCbntDescriptor()</c> 设置实际描述符。
    /// </summary>
    public S7TagCbntBuilder() 
        : base(new TagCbnt(new TagCbntDescriptor { Name = "unkown_s7_cbnt_name", StartAddress = "unknown_s7_cbnt_start_address" }))
    {
    }

    /// <inheritdoc/>
    public override TagCbntBuilderBase AddTags(IList<TagDescriptor> descriptors, ITagChannel channel)
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

    /// <inheritdoc/>
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
            var addr = S7AddressParser.Parse(tag.RawAddress());
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
                tag.TagDescriptor.NormalizedAddress = addr.ToString();
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

