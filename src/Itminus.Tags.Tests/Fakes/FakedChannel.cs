using System.Threading;
using System.Threading.Tasks;

namespace Itminus.Tags.Tests.Fakes;

internal class FakedChannel : ITagChannel
{
    public string ChannelName { get; } = "fake";

    public string Driver => "fake";

    public Task DisconnectAsync(CancellationToken ct) => Task.CompletedTask;

    public void Dispose() { }

    public Task EnsureConnectedAsync(bool force, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
