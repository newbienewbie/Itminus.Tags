
namespace Itminus.Tags.SimpleFiles;

/// <summary>
/// 构建 SimpleFiles 直接测点的构建器。<br/>
/// 当通道的驱动为 <see cref="SimpleFilesNames.DriverName"/> 时，用于构建测点。
/// </summary>
public partial class SimpleTagsDirectTagBuilder : TagBuilderBase
{
    /// <summary>
    /// c'tor
    /// </summary>
    public SimpleTagsDirectTagBuilder()
    {
    }

    /// <inheritdoc/>
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
