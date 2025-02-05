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
    /// 通道
    /// </summary>
    IList<ITagChannel> Channels { get; }

    /// <summary>
    /// 测点
    /// </summary>
    ITagGrp Tags { get; }

    /// <summary>
    /// 挂载逻辑小组件。返回一个可释放对象，用于卸载逻辑小组件。
    /// </summary>
    /// <returns></returns>
    IDisposable Attach();
}
