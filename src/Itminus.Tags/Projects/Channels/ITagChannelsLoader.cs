
namespace Itminus.Tags;


/// <summary>
/// 根据一系列通道描述符，加载 <see cref="ITagChannel"/>
/// </summary>
public interface ITagChannelsLoader
{
    IList<ITagChannel> LoadChannels(IEnumerable<TagChannelDescriptor> descriptors);
}