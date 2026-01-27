
namespace Itminus.Tags.Projects;

public interface ITagsProject
{
    /// <summary>
    /// 项目根目录
    /// </summary>
    string ProjectRoot { get; }

    /// <summary>
    /// 通道
    /// </summary>
    IList<ITagChannel> Channels { get; }

    /// <summary>
    /// 逻辑
    /// </summary>
    IList<ILogicet> Logicets { get; }

    /// <summary>
    /// 测点
    /// </summary>
    ITagGrp Tags { get; }


    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="channelFactory"></param>
    /// <param name="tagsParser"></param>
    /// <param name="logicetLoader"></param>
    void Initialize(ITagChannelFactory channelFactory, ITagsLoader tagsParser, ILogicetLoader logicetLoader);
}