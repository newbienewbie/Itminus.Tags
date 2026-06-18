using Itminus.Tags.ComScanner.Channels;

namespace Itminus.Tags.ComScanner.Tags;

/// <summary>
/// 串口测点构建器。<br/>
/// </summary>
public class ComTagBuilder : TagBuilderBase
{

    public override ITag Build(ITagChannel channel)
    {
        if(channel is null)
        {
            throw new Exception($"测点({this.Name})未配置通道({this.TagDescriptor.TagName})");
        }

        var tagKind = this.TagDescriptor.TagKind;
        if (string.IsNullOrEmpty(tagKind) || string.Compare(tagKind,BuiltinTagKinds.STR, ignoreCase: true) == 0)
        {
            if (channel is not ComChannelBase<string> com)
            {
                throw new InvalidCastException($"测点({this.Name})当前通道必须是{nameof(ComChannelBase<string>)}！实际={channel.GetType()}");
            }

            var tag = new ComStrTag(this.TagDescriptor, com, TagContainer.From(this.Parent));
            return tag;
        }

        throw new NotImplementedException($"串口测点({this.Name})的类型({tagKind})上不支持！");
    }
}
