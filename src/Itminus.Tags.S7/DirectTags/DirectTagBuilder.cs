namespace Itminus.Tags.S7;

internal partial class S7DirectTagBuilder : TagBuilderBase
{
    public S7DirectTagBuilder()
    {
    }

    /// <inheritdoc/>
    protected override ITag Fallback(ITagChannel channel)
    {
        S7TagChannel? s7ch;
        if(this.Channel is null)
        {
            s7ch = null;
        }
        else if (this.Channel is not S7TagChannel)
        {
            throw new Exception($"测点({this.Name})配置了通道({this.Channel.ChannelName()})，但不是{nameof(S7TagChannel)}");
        }
        else
        {
            s7ch= this.Channel as S7TagChannel;
        }

        var factory = new S7DirectTagFactory(this.Parent.IntoTagContainer());
        var tag = factory.Create(this.TagDescriptor, s7ch);

        return tag;
    }
}
