using Itminus.Tags.ModbusTcp;

namespace Itminus.Tags.ZLan;

public class ZLanTagFactory : TagCbntorFactoryBase
{
    private ModbusTcpTagFactory _mbFactory;

    public ZLanTagFactory(TagCbntBuilderBase builder) : base(builder)
    {
        this._mbFactory = new ModbusTcpTagFactory(builder);
    }

    public override ITagCbntor CreateTag(TagDescriptor descriptor)
    {
        var tag = descriptor.TagKind switch
        {
            TagKinds.DI => this._mbFactory.CreateDITag(descriptor) as ITagCbntor,
            TagKinds.DO => this._mbFactory.CreateDOTag(descriptor) as ITagCbntor,
            _ => throw new Exception($"未预料到的测点种类={descriptor.TagKind}")
        };
        return tag;
    }

    protected override int GetTagOffset(TagDescriptor tagDescriptor)
    {
        throw new NotImplementedException();
    }
}
