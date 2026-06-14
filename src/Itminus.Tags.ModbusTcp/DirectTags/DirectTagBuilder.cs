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

        var factory = new ModbusTcpDirectTagFactory( this.Parent.IntoTagContainer());
        var tag = factory.Create(this.TagDescriptor, this.Channel);
        return tag;
    }
}
