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
        if (channel is not ComScannerChannel com)
        {
            throw new InvalidCastException($"测点({this.Name})当前通道必须是{nameof(ComScannerChannel)}！实际={channel.GetType()}");
        }

        var tagkind = this.TagDescriptor.TagKind;
        var tag = new ComCodeScannerTag(this.TagDescriptor, com);
        return tag;
        
        // throw new ArgumentException($"{nameof(CodeScannerTagBuilder)}目前只支持{BuiltinTagKinds.STR}型测点，但是当前测点的类型是{tagkind}(Tag={this.TagDescriptor.TagName})");
    }
}
