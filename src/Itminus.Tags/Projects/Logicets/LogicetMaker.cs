using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Itminus.Tags.Plugins;


internal class LogicetMaker : ILogicetMaker
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<LogicetMaker> _logger;

    public LogicetMaker(IServiceProvider sp, ILogger<LogicetMaker> logger)
    {
        this._sp = sp;
        this._logger = logger;
    }

    public TLogicet? MakeLogicet<TLogicet>(IList<ITagChannel> channels, ITagGrp tags, out string? msg)
        where TLogicet : class, ILogicet
    {
        try
        {
            TLogicet logicet = ActivatorUtilities.CreateInstance<TLogicet>(_sp, channels, tags);
            msg = null;
            return logicet;
        }
        catch (Exception ex)
        {
            var t = typeof(TLogicet);
            msg = $"加载Logicet失败：{t}。异常={ex.Message}";
            return null;
        }
    }

    public ILogicet? MakeLogicet(Type logicetType, IList<ITagChannel> channels, ITagGrp tags, out string? msg)
    {
        try
        {
            ILogicet logicet = (ActivatorUtilities.CreateInstance(this._sp, logicetType, channels, tags) as ILogicet)!;
            msg = null;
            return logicet;
        }
        catch (Exception ex)
        {
            var assembly = logicetType.Assembly;
            msg = $"热加载Logicet失败：{logicetType.Name}, 程序集={assembly.Location}。异常={ex.Message}";
            return null;
        }
    }
}
