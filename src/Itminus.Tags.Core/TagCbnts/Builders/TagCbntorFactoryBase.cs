namespace Itminus.Tags;


/// <summary>
/// 组合测点工厂基类。用于在测定组合构建器中创建测点组合子
/// </summary>
public abstract class TagCbntorFactoryBase
{

    /// <summary>
    /// c'tor
    /// </summary>
    /// <param name="cbntBuilder"></param>
    public TagCbntorFactoryBase(TagCbntBuilderBase cbntBuilder)
    {
        CbntBuilder = cbntBuilder;
    }

    /// <summary>
    /// 测点组合构建器
    /// </summary>
    public TagCbntBuilderBase CbntBuilder { get; }

    /// <summary>
    /// 测点组合
    /// </summary>
    public ITagCbnt TagCbnt => CbntBuilder.TagCbnt;


    /// <summary>
    /// 根据描述，创建Tag
    /// </summary>
    /// <param name="tagDescriptor"></param>
    /// <returns></returns>
    public abstract ITag CreateTag(TagDescriptor tagDescriptor);


}
