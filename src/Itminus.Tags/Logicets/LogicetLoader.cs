using Itminus.Tags.Core.Projects;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Itminus.Tags.Logicets;

/// <summary>
/// Logicet 加载器。
/// </summary>
internal class LogicetLoader : ILogicetsLoader
{
    private readonly LogicetLoadOptions _options;
    private readonly ILogger<LogicetLoader> _logger;

    /// <summary>
    /// c'tor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public LogicetLoader(IOptions<LogicetLoadOptions> options, ILogger<LogicetLoader> logger)
    {
        this._options = options.Value;
        this._logger = logger;
    }


    /// <inheritdoc/>
    public LoadedLogicets LoadLogicets(IServiceProvider sp, IEnumerable<string> dllLocations, IReadOnlyList<ITagChannel> channels, ITagGrp tags)
    {
        var disposables = new List<IDisposable>();
        var logicets = new List<ILogicet>();

        foreach (var dll in dllLocations)
        {

            // 
            IList<ILogicet> batch;
            IDisposable? loader = null;
            try
            {
                List<Type> sharedTypes = new List<Type> {
                    typeof(ILogicet),
                    typeof(ITagChannel),
                    typeof(ITagGrp),
                    typeof(IServiceProvider),
                    typeof(IServiceCollection),
                    typeof(ILogger),
                };

                (batch, loader) = MakeCore(sp, channels, tags, dll, sharedTypes);
                logicets.AddRange(batch);
                disposables.Add(loader);
            }
            catch (Exception ex)
            {
                this._logger.LogError("加载Logicet失败：dll={dll}, ex={ex}, strace={strace}", dll, ex.Message, ex.StackTrace);
                if(loader is not null)
                {
                    try
                    {
                        loader.Dispose();
                    }
                    catch
                    {
                        /* 有意忽略 */
                    }
                }
            }

        }
        return new LoadedLogicets(logicets, disposables);
    }

    private (IList<ILogicet> batch, IDisposable loader) MakeCore(IServiceProvider sp, IReadOnlyList<ITagChannel> channels, ITagGrp tags,string dll, List<Type> sharedTypes)
    {
        this._options.SharedTypesFilter?.Invoke(dll, sharedTypes);
        var loader = PluginLoader.CreateFromAssemblyFile(
            dll,
            isUnloadable: true,
            sharedTypes: [.. sharedTypes]
        );
        var plugin = loader.LoadDefaultAssembly();
        var batch = MakeLogicets(sp, plugin, channels, tags);

        return (batch, loader);
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
                var (logicet, ex) = LogicetProviderUtils.CreateLogicet(sp, t, channels, tags); 
                if (logicet is null)
                {
                    _logger.LogError("构建Logicet错误：t={t}, ex={ex}, strace={strace}", t.Name, ex?.Message, ex?.StackTrace);
                    return null;
                }
                return logicet;
            })
            .Where(logicet => logicet != null)
            .ToList();
        return logicets!;
    }


}
