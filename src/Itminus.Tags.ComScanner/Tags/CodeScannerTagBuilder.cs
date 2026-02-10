using Itminus.Tags.ComScanner.Channels;

namespace Itminus.Tags.ComScanner.Tags;

public class CodeScannerTagBuilder : TagBuilderBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CodeScannerTagBuilder()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    public override TagDescriptor TagDescriptor { get; protected set ; }
    public override ITagChannel Channel { get; protected set; }

    public override ITag Build()
    {
        if(this.Channel is null)
        {
            throw new Exception($"测点({this.Name})未配置通道({this.TagDescriptor.TagName})");
        }
        if (this.Channel is not ComScannerChannel channel)
        {
            throw new InvalidCastException($"测点({this.Name})当前通道必须是{nameof(ComScannerChannel)}！实际={this.Channel.GetType()}");
        }


        var tagkind = this.TagDescriptor.TagKind;
        if(tagkind == BuiltinTagKinds.STR)
        {
            var tag = new ComCodeScannerTag(this.TagDescriptor, channel);
            return tag;
        }
        else
        {
            throw new ArgumentException($"{nameof(CodeScannerTagBuilder)}目前只支持{BuiltinTagKinds.STR}型测点，但是当前测点的类型是{tagkind}(Tag={this.TagDescriptor.TagName})");
        }
    }
}
