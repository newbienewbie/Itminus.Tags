
namespace Itminus.Tags.Plugins;

public interface ILogicetMaker
{
    ILogicet? MakeLogicet(Type logicetType, IList<ITagChannel> channels, ITagGrp tags, out string? msg);

    TLogicet? MakeLogicet<TLogicet>(IList<ITagChannel> channels, ITagGrp tags, out string? msg)
        where TLogicet : class, ILogicet;
}