namespace Itminus.Tags.Logicets;

public static class TagProjectExtensions
{
    /// <summary>
    /// 注册逻辑
    /// </summary>
    /// <typeparam name="TLogicet"></typeparam>
    /// <param name="project"></param>
    /// <param name="sp"></param>
    /// <returns></returns>
    public static bool TryAddLogicet<TLogicet>(this ITagsProject project, IServiceProvider sp)
        where TLogicet : ILogicet
    {
        var logicet = LogicetProviderUtils.CreateLogicet(sp, typeof(TLogicet), project.Channels, project.Tags);
        if(logicet is null)
        {
            return false;
        }

        project.Logicets.Add(logicet);
        return true;
    }
}