using Itminus.Tags.Core.Projects;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Itminus.Tags.Logicets;

/// <summary>
/// Logicet 加载器。
/// </summary>
internal class LogicetLoader : ILogicetsLoader
{
   
    private readonly ILogger<LogicetLoader> _logger;

    public LogicetLoader(ILogger<LogicetLoader> logger)
    {
        _logger = logger;
    }


    /// <inheritdoc/>
    public IList<ILogicet> LoadLogicets(IServiceProvider sp, IEnumerable<string> dllLocations, IReadOnlyList<ITagChannel> channels, ITagGrp tags)
    {
        var results = dllLocations
            .SelectMany(l =>
            {
                var loader = PluginLoader.CreateFromAssemblyFile(
                    l,
                    isUnloadable: true,
                    sharedTypes: new[] { 
                        typeof(ILogicet), 
                        typeof(ITagChannel), 
                        typeof(ITagGrp) 
                    }
                );
                var plugin = loader.LoadDefaultAssembly();
                var logicets = MakeLogicets(sp, plugin, channels, tags);
                return logicets;
            })
            .ToList();
        return results;
    }


    /// <summary>
    /// 从程序集创建 Logicet 实例列表。对于无法成功创建实例的类型，会跳过。
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="assembly"></param>
    /// <param name="channels"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    protected virtual IList<ILogicet> MakeLogicets(IServiceProvider sp, Assembly assembly, IReadOnlyList<ITagChannel> channels, ITagGrp tags)
    {
        var types = assembly.GetTypes()
            .Where(t => 
                !t.IsInterface && !t.IsAbstract && !t.IsGenericType 
                && typeof(ILogicet).IsAssignableFrom(t)
            );
        
        var logicets = types
            .Select(t => {
                var logicet = LogicetProviderUtils.CreateLogicet(sp, t, channels, tags); 
                if (logicet is null)
                {
                    _logger.LogError("构建Logicet错误：t={t}", t.Name);
                    return null;
                }
                return logicet;
            })
            .Where(t => t != null)
            .ToList();
        return logicets!;
    }


}
