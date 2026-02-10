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
        // normalize the tagsize
        if (tagDescriptor.TagSize == 0)
        {
            tagDescriptor.TagSize = 1;
        }
        var tagAddr = PinAddrUtils.ParseDI(tagDescriptor.Address);
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
        var tagAddr = PinAddrUtils.ParseDO(tagDescriptor.Address);
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

    protected override int GetTagOffset(TagDescriptor tagDescriptor)
    {
        throw new NotImplementedException();
    }
}
