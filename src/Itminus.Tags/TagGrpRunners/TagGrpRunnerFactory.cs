using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Itminus.Tags;

internal class TagGrpRunnerFactory : ITagGrpRunnerFactory
{
    private readonly IServiceProvider _sp;

    /// <summary>
    /// c'tor
    /// </summary>
    public TagGrpRunnerFactory(IServiceProvider sp)
    {
        this._sp = sp;
    }

    /// <inheritdoc/>
    public ITagGrpRunner Create(ITagsProject project)
    {
        var logger = this._sp.GetRequiredService<ILogger<TagGrpRunner>>();
        var strategy = this._sp.GetService<ITagGrpRunnerRetryStrategy>();
        var runner = new TagGrpRunner(project, logger, strategy);
        return runner;
    }
}
