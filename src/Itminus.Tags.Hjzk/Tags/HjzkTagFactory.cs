using Itminus.Tags.ModbusTcp.Tags;

namespace Itminus.Tags.Hjzk;

public class HjzkTagFactory : TagCbntorFactoryBase
{
    private readonly HjzkCbntBuilderBase _cbntBuilder;

    public HjzkTagFactory(HjzkCbntBuilderBase builder) : base(builder)
    {
        this._cbntBuilder = builder;
    }

    public virtual DITag CreateDITag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 1;
        }
        var tagAddr = PinAddrUtils.TryParseDI(tagDescriptor.RawAddress, out var addr) ?
            addr : 
            throw new ArgumentException($"Hjzk DI 地址非法({tagDescriptor.RawAddress})");
        tagDescriptor.NormalizedAddress = addr.ToModbusTcpAddr(this._cbntBuilder.Slave);

        var startAddr = DIPinAddr.DI1;
        var offset = (int)tagAddr - (int)startAddr;
        return new DITag(tagDescriptor, TagCbnt, offset);
    }


    public virtual DOTag CreateDOTag(TagDescriptor tagDescriptor)
    {
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 1;
        }
        var tagAddr = PinAddrUtils.TryParseDO(tagDescriptor.RawAddress, out var addr) ?
            addr:  
            throw new ArgumentException($"Hjzk DO 地址非法({tagDescriptor.RawAddress})");
        tagDescriptor.NormalizedAddress = addr.ToModbusTcpAddr(this._cbntBuilder.Slave);

        var startAddr = DOPinAddr.DO1;
        var offset = (int)tagAddr - (int)startAddr;
        return new DOTag(tagDescriptor, TagCbnt, offset);
    }

    public override ITagCbntor CreateTag(TagDescriptor descriptor)
    {
        var tag = descriptor.TagKind switch
        {
            BuiltinTagKinds.DI => this.CreateDITag(descriptor) as ITagCbntor,
            BuiltinTagKinds.DO => this.CreateDOTag(descriptor) as ITagCbntor,
            _ => throw new Exception($"未预料到的测点种类={descriptor.TagKind}")
        };
        return tag;
    }

}
