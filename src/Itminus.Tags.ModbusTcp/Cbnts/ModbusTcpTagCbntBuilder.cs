using Itminus.Tags.TagCbntors;

namespace Itminus.Tags.ModbusTcp;

/// <summary>
/// ModbusTcp Cbnt 构建器
/// </summary>
public class ModbusTcpTagCbntBuilder : TagCbntBuilderBase
{
    /// <summary>
    /// c'tor<br/>
    /// 需要额外使用 <c>WithCbntDescriptor()</c> 设置实际描述符。
    /// </summary>
    public ModbusTcpTagCbntBuilder()
        : base(new TagCbnt(new TagCbntDescriptor { Name = "unkown_modbustcp_cbnt_name", StartAddress = "unknown_modbustcp_cbnt_start_address" }))
    {
    }


    /// <summary>
    /// 从站站号
    /// </summary>
    public virtual byte Slave { get; protected set; } = 1;

    /// <summary>
    /// 区域
    /// </summary>
    public virtual string? Area { get; protected set; }


    /// <inheritdoc/>
    public override TagCbntBuilderBase WithCbntDescriptor(TagCbntDescriptor descriptor)
    {
        if (descriptor.Extras.TryGetValue("slave", out var slaveAttr))
        {
            if (!byte.TryParse(slaveAttr.Value, out var slave))
            {
                throw new ArgumentException($"无效的Modbus从站地址:{slaveAttr.Value}");
            }
            this.Slave = slave;
        }
        if (descriptor.Extras.TryGetValue("area", out var areaAttr))
        {
            this.Area = areaAttr.Value;
        }
        base.WithCbntDescriptor(descriptor);
        return this;
    }

    /// <inheritdoc/>
    protected override ITagCbntor Fallback(TagDescriptor descriptor, ITagChannel channel)
    {
        var tagFactory = this.MakeModbusTcpTagFactory();
        return tagFactory.CreateTag(descriptor);
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
        return this;
    }
}