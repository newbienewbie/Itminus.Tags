using Itminus.Tags.ComScanner.Channels;

namespace Itminus.Tags.ComScanner.Tags;

public class CodeScannerTagBuilder : TagBuilderBase
{

    public override ITag Build(ITagChannel channel)
    {
        if(channel is null)
        {
            throw new Exception($"测点({this.Name})未配置通道({this.TagDescriptor.TagName})");
        }
        if (channel is not ComLineScannerChannel com)
        {
            throw new InvalidCastException($"测点({this.Name})当前通道必须是{nameof(ComLineScannerChannel)}！实际={channel.GetType()}");
        }

        var tag = new ComCodeScannerTag(this.TagDescriptor, com, TagContainer.From(this.Parent));
        return tag;
    }
}
