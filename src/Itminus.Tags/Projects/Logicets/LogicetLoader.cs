using Itminus.Tags.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Itminus.Tags.Projects;


/// <summary>
/// Logicet 加载器
/// </summary>
public class LogicetLoader : ILogicetLoader
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<LogicetLoader> _logger;

    public LogicetLoader(IServiceProvider sp, ILogger<LogicetLoader> logger)
    {
        this._sp = sp;
        this._logger = logger;
    }


    /// <inheritdoc/>
    public IList<ILogicet> LoadLogicets(IEnumerable<string> dllLocations, IList<ITagChannel> channels, ITagGrp tags)
    {
        var results = dllLocations
            .SelectMany(l =>
            {
                var plugin = LoadPlugin(l);
                var logicets = this.MakeLogicets(plugin, channels, tags);
                return logicets;
            })
            .ToList();
        return results;
    }

    protected virtual Assembly LoadPlugin(string pluginLocation)
    {
        PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
        return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
    }

    protected virtual IList<ILogicet> MakeLogicets(Assembly assembly, IList<ITagChannel> channels, ITagGrp tags)
    {
        var types = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract && !t.IsGenericType);
        var logicetMaker = this._sp.GetRequiredService<ILogicetMaker>();
        
        var logicets = types
            .Select(t => {
                var logicet = logicetMaker.MakeLogicet(t, channels, tags, out var msg);
                if (logicet is null)
                {
                    this._logger.LogError(msg);
                    return null;
                }
                return logicet;
            })
            .Where(t => t != null)
            .ToList();
        return logicets!;
    }
}
