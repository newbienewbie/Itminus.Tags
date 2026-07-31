
namespace Itminus.Tags.SimpleFiles;

/// <summary>
/// 委托：创建 SimpleFiles 直接测点
/// </summary>
/// <param name="descriptor"></param>
/// <param name="thisChannel"></param>
/// <param name="container"></param>
/// <returns></returns>
public delegate ITag CreateSimpleFilesDirectTag(
    TagDescriptor descriptor, 
    SimpleFilesTagChannel? thisChannel,
    TagContainer container
    );

/// <summary>
/// 构建 SimpleFiles 直接测点的构建器。<br/>
/// 当通道的驱动为 <see cref="SimpleFilesNames.DriverName"/> 时，用于构建测点。
/// </summary>
public partial class SimpleFilesDirectTagBuilder : TagBuilderBase
{
    /// <summary>
    /// c'tor
    /// </summary>
    public SimpleFilesDirectTagBuilder()
    {
    }

    private CreateSimpleFilesDirectTag? _createTag;

    /// <summary>
    /// 设置创建 SimpleFiles 直接测点的委托。<br/>
    /// </summary>
    /// <param name="createTag"></param>
    /// <returns></returns>
    public SimpleFilesDirectTagBuilder WithFactory(CreateSimpleFilesDirectTag createTag)
    {
        this._createTag = createTag;
        return this;
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
        
        if(this._createTag is not null)
        {
            var tag = this._createTag(this.TagDescriptor, ch, TagContainer.From(this.Parent));
            if(tag is not null)
            {
                return tag;
            }
        }

        return Fallback(ch);
    }

    private ITag Fallback(SimpleFilesTagChannel? ch)
    {
        var factory = new SimpleFilesDirectTagFactory(this.Parent.IntoTagContainer());
        var tag = factory.Create(this.TagDescriptor, ch);

        return tag;
    }
}
