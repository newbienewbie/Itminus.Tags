namespace Itminus.Tags.OpcUaClient.DirectTags;

internal class OpcUaClientTagBuilder : TagBuilderBase
{
    /// <inheritdoc/>
    protected override ITag Fallback(ITagChannel channel)
    {
        var container = this.Parent.IntoTagContainer();

        OpcUaClientTagChannel? occh;
        if (this.Channel is null)
        {
            occh = null;
        }
        else if (this.Channel is not OpcUaClientTagChannel)
        {
            throw new Exception($"测点({this.Name})配置了通道，但不是{nameof(OpcUaClientTagChannel)}");
        }
        else
        {
            occh = this.Channel as OpcUaClientTagChannel;
        }

        return new OpcUaClientDirectTag(this.TagDescriptor, occh, container);
    }
}