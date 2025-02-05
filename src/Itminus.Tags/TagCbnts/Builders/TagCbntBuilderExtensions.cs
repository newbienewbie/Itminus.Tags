namespace Itminus.Tags;

public static class TagCbntBuilderExtensions
{

    public static TagCbntBuilderBase ConfigureCbnt(this TagCbntBuilderBase builder, TagsCbntConfiguration config)
    {
        return builder
            .WithName(config.Name)
            .WithIsEnabled(config.IsEnabled)
            .WithInterval(config.ScanInterval)
            .AddTags(config.TagDescriptors);
    }
}