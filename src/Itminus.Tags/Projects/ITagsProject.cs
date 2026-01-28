
using System.Xml.Linq;

namespace Itminus.Tags.Projects;

public interface ITagsProject
{
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
    /// 项目根目录
    /// </summary>
    string? ProjectRoot { get; }

    /// <summary>
    /// 轮询开始
    /// </summary>
    event TurnStarted? TurnStarted;
    
    /// <summary>
    /// 轮询崩溃
    /// </summary>
    event TurnCrashed? TurnCrashed;

    /// <summary>
    /// 初始化，如果root为空，则默认取 projRoot下的index.xml文件
    /// </summary>
    /// <param name="projRoot"></param>
    /// <param name="root"></param>
    void Initialize(string projRoot, XElement? root=null);

    /// <summary>
    /// 运行
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task RunAsync(CancellationToken ct);
}