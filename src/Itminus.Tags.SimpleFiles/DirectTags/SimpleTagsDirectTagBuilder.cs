
namespace Itminus.Tags.SimpleFiles;

internal partial class SimpleTagsDirectTagBuilder : TagBuilderBase
{
    public SimpleTagsDirectTagBuilder()
    {
    }


    public override ITag Build(ITagChannel channel)
    {
        SimpleFilesTagChannel? ch;
        if (this.Channel is null)
        {
            ch = null;
        }
        else if (this.Channel is not SimpleFilesTagChannel)
        {
            throw new Exception($"测点({this.Name})配置了通道({this.Channel.ChannelName()})，但不是{nameof(SimpleFilesTagChannel)}");
        }
        else
        {
            ch = this.Channel as SimpleFilesTagChannel;
        }

        var factory = new SimpleFilesDirectTagFactory(this.Parent.IntoTagContainer());
        var tag = factory.Create(this.TagDescriptor, ch);

        return tag;
    }
}
