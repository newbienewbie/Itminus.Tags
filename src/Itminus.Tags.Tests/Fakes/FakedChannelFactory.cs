using System.Collections.Generic;

namespace Itminus.Tags.Tests.Fakes;

internal class FakedChannelFactory : ITagChannelFactory
{
    public FakedChannelFactory()
    {
    }


    public ITagChannel Create(TagChannelDescriptor chDescriptor)
    {
        return new FakedChannel();
    }


    private static IReadOnlyList<string> _drivers = new List<string>() { "fake" };


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IReadOnlyList<string> GetAvailableDrivers() => _drivers;
}