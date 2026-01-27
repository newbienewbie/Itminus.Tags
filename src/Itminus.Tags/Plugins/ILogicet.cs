using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags;


/// <summary>
/// 逻辑小组件，用于对测点添加逻辑
/// </summary>
public interface ILogicet
{
    /// <summary>
    /// 运行顺序
    /// </summary>
    int Order { get; }

    /// <summary>
    /// 通道
    /// </summary>
    IList<ITagChannel> Channels { get; }

    /// <summary>
    /// 测点
    /// </summary>
    ITagGrp Tags { get; }

    /// <summary>
    /// 是否能匹配入口？
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    bool MatchEntry(ITagGrp entry);

    /// <summary>
    /// 处理
    /// </summary>
    /// <returns></returns>
    Task ProcessAsync(ITagGrp entry, ITagChannel thisChannel);
}
