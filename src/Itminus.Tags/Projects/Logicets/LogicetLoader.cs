using Microsoft.Extensions.DependencyInjection;
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

    public LogicetLoader(IServiceProvider sp)
    {
        this._sp = sp;
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

        var logicets = types
            .Select(t =>
            {
                ILogicet logicet = (ActivatorUtilities.CreateInstance(this._sp, t, channels, tags) as ILogicet)!;
                return logicet;
            })
            .Where(t => t != null)
            .ToList();
        return logicets;
    }
}
