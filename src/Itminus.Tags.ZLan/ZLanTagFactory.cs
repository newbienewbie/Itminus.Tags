using Itminus.Tags.ModbusTcp;

namespace Itminus.Tags.ZLan
{
    public class ZLanTagFactory : ModbusTcpTagFactory
    {
        public ZLanTagFactory(TagCbntBuilderBase builder) : base(builder)
        {
        }


    }


    public static class ZLanTagFactoryExtensions
    {
        public static ITagCbntor AddDI(this ZLanTagFactory factory, string tagName, DIPinAddr pin)
        {
            return factory.CreateDITag(new TagDescriptor()
            {
                TagName = tagName,
                Address = pin.ToModbusTcpAddr(),
                TagKind = TagKinds.BIT,
                TagSize = 1,
            });
        }

        public static ITagCbntor AddDO(this ZLanTagFactory factory, string tagName, DOPinAddr pin)
        {
            return factory.CreateDOTag(new TagDescriptor()
            {
                TagName = tagName,
                Address = pin.ToModbusTcpAddr(),
                TagKind = TagKinds.BIT,
                TagSize = 1,
            });
        }
    }
}
