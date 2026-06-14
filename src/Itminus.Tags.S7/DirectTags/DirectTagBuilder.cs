namespace Itminus.Tags.S7;

internal partial class S7DirectTagBuilder : TagBuilderBase
{
    public S7DirectTagBuilder()
    {
    }


    public override ITag Build(ITagChannel channel)
    {

        if (this.Channel is not null && this.Channel is not S7TagChannel)
        {
            throw new Exception($"测点({this.Name})配置了通道，但不是{nameof(S7TagChannel)}");
        }

        var factory = new S7DirectTagFactory(this.Parent.IntoTagContainer());
       
        // 这个时候 tag 尚未构造完成，尚未关联到 Parent，不可以使用 tag.GetRequiredChannel()，必须依赖外部传入的 channel 参数
        var s7Channel = channel as S7TagChannel;
        if(s7Channel is null)
        {
            throw new Exception($"测点({this.Name})配置的通道不是{nameof(S7TagChannel)}");
        }
        var tag = factory.Create(this.TagDescriptor, this.Channel);

        return tag;
    }
}
