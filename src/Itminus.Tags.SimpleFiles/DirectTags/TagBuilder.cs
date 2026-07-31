
namespace Itminus.Tags.SimpleFiles;

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


    /// <summary>
    /// 使用 JSON 测点工厂创建<see cref="JsonDirectTag{TVal}"/>型测点。<br/>
    /// </summary>
    /// <typeparam name="TVal"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public SimpleFilesDirectTagBuilder WithJsonTagFactory<TVal>()
    {
        this.WithFactory( 
            (descriptor, thisChannel, container) => {
                var channel = thisChannel ?? container.SearchRequiredChannel();
                var sfsChannel = channel as SimpleFilesTagChannel ?? throw new InvalidOperationException($"通道类型不匹配：{channel?.GetType().FullName}");
                descriptor.NormalizedAddress = sfsChannel.MakePath(descriptor.RawAddress);
                return new JsonDirectTag<TVal>(descriptor, thisChannel as SimpleFilesTagChannel, container);
            }
        );
        return this;
    }


    /// <inheritdoc/>
    protected override ITag Fallback(ITagChannel ch)
    {
        var factory = new SimpleFilesDirectTagFactory(this.Parent.IntoTagContainer());
        var tag = factory.Create(this.TagDescriptor, this.Channel as SimpleFilesTagChannel);

        return tag;
    }
}
