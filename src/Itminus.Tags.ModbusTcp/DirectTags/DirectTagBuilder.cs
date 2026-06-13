namespace Itminus.Tags.ModbusTcp;

internal partial class ModbusTcpDirectTagBuilder : TagBuilderBase
{
    public ModbusTcpDirectTagBuilder()
    {
    }


    public override ITag Build(ITagChannel channel)
    {

        if (this.Channel is not null && this.Channel is not ModbusTcpChannel)
        {
            throw new Exception($"测点({this.Name})配置了通道，但不是{nameof(ModbusTcpChannel)}");
        }

        var factory = new ModbusTcpDirectTagFactory();
       
        // 这个时候 tag 尚未构造完成，尚未关联到 Parent，不可以使用 tag.GetRequiredChannel()，必须依赖外部传入的 channel 参数
        var ch = channel as ModbusTcpChannel;
        if(ch is null)
        {
            throw new Exception($"测点({this.Name})配置的通道不是{nameof(ModbusTcpChannel)}");
        }
        var tag = factory.Create(this.TagDescriptor, this.Channel, ch);

        return tag;
    }
}
