using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Itminus.Tags;

internal class TagChannelsLoader : ITagChannelsLoader
{
    private readonly ITagChannelFactory _channelFactory;

    public TagChannelsLoader(ITagChannelFactory channelFactory)
    {
        this._channelFactory = channelFactory;
    }


    /// <inheritdoc/>
    public virtual IList<ITagChannel> LoadChannels(IEnumerable<TagChannelDescriptor> descriptors)
    {
        var channels = descriptors
            .Select(this._channelFactory.Create)
            .ToList();
        return channels;
    }


}
