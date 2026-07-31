using Itminus.Tags.ComScanner.Channels;
using System.Text;

namespace Itminus.Tags.ComScanner.Tags;

/// <summary>
/// 委托：创建串口测点。<br/>
/// 委托会被优先调用；如果返回 null，则回退到构建器内部默认逻辑。
/// </summary>
/// <param name="descriptor"></param>
/// <param name="thisChannel">自身的通道(不冒泡)</param>
/// <param name="container">测点所属的容器</param>
/// <returns></returns>
public delegate ITag? CreateComTag(
    TagDescriptor descriptor,
    ITagChannel? thisChannel,
    TagContainer container
    );

/// <summary>
/// 串口测点构建器。<br/>
/// </summary>
public class ComTagBuilder : TagBuilderBase
{
    private CreateComTag? _createTag;

    /// <summary>
    /// 设置创建串口测点的委托。<br/>
    /// 委托会被优先用于创建测点；若未设置委托，或委托返回 null，则回退到内部默认逻辑（仅支持 STR 类型）。
    /// </summary>
    /// <param name="createTag"></param>
    /// <returns></returns>
    public ComTagBuilder WithFactory(CreateComTag createTag)
    {
        this._createTag = createTag;
        return this;
    }

    /// <inheritdoc/>
    public override ITag Build(ITagChannel channel)
    {
        if(channel is null)
        {
            throw new Exception($"测点({this.Name})未配置通道({this.TagDescriptor.TagName})");
        }

        if (this._createTag is not null)
        {
            var tag = this._createTag(this.TagDescriptor, this.Channel, TagContainer.From(this.Parent));
            if (tag is not null)
            {
                return tag;
            }
        }

        return Fallback(channel);
    }

    /// <summary>
    /// 内部默认逻辑：仅支持 STR 类型只读或者只写串口Tag
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    protected virtual ITag Fallback(ITagChannel channel)
    {
        var tagKind = this.TagDescriptor.TagKind;
        if (string.IsNullOrEmpty(tagKind) || string.Compare(tagKind,BuiltinTagKinds.STR, ignoreCase: true) == 0)
        {
            if (channel is not ComChannelBase<string> com)
            {
                throw new InvalidCastException($"测点({this.Name})当前通道必须是{nameof(ComChannelBase<string>)}！实际={channel.GetType()}");
            }

            var accessMode = this.TagDescriptor.AccessMode ?? this.Parent.SearchAccessMode();

            ITag tag = accessMode switch {
                TagAccessMode.RO => new ComReadOnlyTag<string>(this.TagDescriptor, com, TagContainer.From(this.Parent)),
                TagAccessMode.WO => new ComWriteOnlyTag<string>(this.TagDescriptor, com, TagContainer.From(this.Parent), converter: str => Encoding.UTF8.GetBytes(str)),
                _ => throw new InvalidOperationException($"串口型测点({this.Name})只支持(RO|WO)访问，当前模式={accessMode}！")
            };
            return tag;
        }

        throw new NotImplementedException($"串口测点({this.Name})的类型({tagKind})上不支持！");
    }
}
