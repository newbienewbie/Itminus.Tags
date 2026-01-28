
namespace Itminus.Tags.Projects;


/// <summary>
/// 根据一系列通道描述符，加载 <see cref="ITagChannel"/>
/// </summary>
public interface IChannelsLoader
{
    IList<ITagChannel> LoadChannels(IEnumerable<ChannelDescriptor> descriptors);
}