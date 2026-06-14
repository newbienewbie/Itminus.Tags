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
        var tag = factory.Create(this.TagDescriptor, this.Channel);

        return tag;
    }
}
