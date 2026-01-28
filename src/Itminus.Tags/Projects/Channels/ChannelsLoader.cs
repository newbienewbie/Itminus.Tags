using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.Projects;

internal class ChannelsLoader : IChannelsLoader
{
    private readonly IChannelFactory _channelFactory;

    public ChannelsLoader(IChannelFactory channelFactory)
    {
        this._channelFactory = channelFactory;
    }


    /// <inheritdoc/>
    public virtual IList<ITagChannel> LoadChannels(IList<ChannelDescriptor> descriptors)
    {
        var channels = descriptors
            .Select(this._channelFactory.Create)
            .ToList();
        return channels;
    }
}
