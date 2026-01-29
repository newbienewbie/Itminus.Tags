using Itminus.Tags.ModbusTcp;
using Itminus.Tags.ModbusTcp.Tags;

namespace Itminus.Tags.ZLan;

public class ZLanTagFactory : TagCbntorFactoryBase
{

    public ZLanTagFactory(TagCbntBuilderBase builder) : base(builder)
    {
    }

    public virtual DITag CreateDITag(TagDescriptor tagDescriptor)
    {
        var tagAddr = PinAddrUtils.ParseDI(tagDescriptor.Address);
        return new DITag(tagDescriptor, TagCbnt, 0);
    }


    public virtual DOTag CreateDOTag(TagDescriptor tagDescriptor)
    {
        var tagAddr = PinAddrUtils.ParseDO(tagDescriptor.Address);
        return new DOTag(tagDescriptor, TagCbnt, 0);
    }

    public override ITagCbntor CreateTag(TagDescriptor descriptor)
    {
        var tag = descriptor.TagKind switch
        {
            TagKinds.DI => this.CreateDITag(descriptor) as ITagCbntor,
            TagKinds.DO => this.CreateDOTag(descriptor) as ITagCbntor,
            _ => throw new Exception($"未预料到的测点种类={descriptor.TagKind}")
        };
        return tag;
    }

    protected override int GetTagOffset(TagDescriptor tagDescriptor)
    {
        throw new NotImplementedException();
    }
}
