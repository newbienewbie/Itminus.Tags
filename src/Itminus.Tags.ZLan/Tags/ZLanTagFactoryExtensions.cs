namespace Itminus.Tags.ZLan;

public static class ZLanTagFactoryExtensions
{
    /// <summary>
    /// 这是为了以编程方式而设计的接口，可以通过指定一个强类型的地址，来增加DI测点
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="tagName"></param>
    /// <param name="slave"></param>
    /// <param name="pin"></param>
    /// <returns></returns>
    public static ITagCbntor AddDI(this ZLanTagFactory factory, string tagName, byte slave, DIPinAddr pin)
    {
        return factory.CreateTag(new TagDescriptor()
        {
            TagName = tagName,
            Address = pin.ToModbusTcpAddr(slave),
            TagKind = BuiltinTagKinds.BIT,
            TagSize = 1,
        });
    }

    /// <summary>
    /// 这是为了以编程方式而设计的接口，可以通过指定一个强类型的地址，来增加DO测点
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="tagName"></param>
    /// <param name="slave"></param>
    /// <param name="pin"></param>
    /// <returns></returns>
    public static ITagCbntor AddDO(this ZLanTagFactory factory, string tagName, byte slave, DOPinAddr pin)
    {
        return factory.CreateTag(new TagDescriptor()
        {
            TagName = tagName,
            Address = pin.ToModbusTcpAddr(slave),
            TagKind = BuiltinTagKinds.BIT,
            TagSize = 1,
        });
    }
}
