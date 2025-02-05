using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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
    public IList<ILogicet> LoadLogicets(string indexPath, IList<ITagChannel> channels, ITagGrp tags)
    {
        var locations = ParseLogicetsIndex(indexPath);

        var results = locations
            .SelectMany(l =>
            {
                var plugin = LoadPlugin(l);
                var logicets = this.MakeLogicets(plugin, channels, tags);
                return logicets;
            })
            .ToList();
        return results;
    }

    private static IList<string> ParseLogicetsIndex(string indexPath)
    {
        if (!File.Exists(indexPath))
        {
            throw new Exception($"指定的逻辑组件索引文件路径不存在({indexPath})");
        }

        var dir = Path.GetDirectoryName(indexPath) ?? throw new Exception($"无法获取逻辑组件索引文件所在目录");
        var stream = new FileStream(indexPath, FileMode.Open);
        var locations = JsonSerializer.Deserialize<IList<string>>(stream)
            ?? throw new Exception($"非法的逻辑组件索引。文件路径={indexPath}");
        return locations.Select(l => Path.Combine(dir, l)).ToList();
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
