

using System.Runtime.CompilerServices;

namespace Itminus.Tags;

public static class ITagGrpExtensions
{
    #region 获取子孙节点
    /// <summary>
    /// 以路径获取子节点并作为<see cref="ITag"/>返回。<br/>
    /// 如果路径不存在，会抛出异常；如果对应节点不是 <see cref="ITag"/>，也会抛出异常<br/>
    /// </summary>
    /// <param name="grp"></param>
    /// <param name="path">以“/”分隔</param>
    /// <returns></returns>
    public static ITag SelectTag(this ITagGrp grp, string path)
    {
        var tagunion = grp.Descendant(path);
        return tagunion.AsTag();
    }

    /// <summary>
    /// 以路径获取子节点并作为<see cref="ITagCbnt"/>返回。<br/>
    /// 如果路径不存在，会抛出异常；如果对应节点不是 <see cref="ITagCbnt"/>，也会抛出异常<br/>
    /// </summary>
    /// <param name="grp"></param>
    /// <param name="path">以“/”分隔</param>
    /// <returns></returns>
    public static ITagCbnt SelectCbnt(this ITagGrp grp, string path)
    {
        var tagunion = grp.Descendant(path);
        return tagunion.AsTagCbnt();
    }

    /// <summary>
    /// 以路径获取子节点并作为<see cref="ITagGrp"/>返回。<br/>
    /// 如果路径不存在，会抛出异常；如果对应节点不是 <see cref="ITagGrp"/>，也会抛出异常<br/>
    /// </summary>
    /// <param name="grp"></param>
    /// <param name="path">以“/”分隔</param>
    /// <returns></returns>
    public static ITagGrp SelectGrp(this ITagGrp grp, string path)
    {
        var tagunion = grp.Descendant(path);
        return tagunion.AsTagGrp();
    }
    #endregion

    #region
    public static IList<ITagGrp> ScanEntries(this ITagGrp grp)
    {
        if(grp.IsEntry)
        {
            return new List<ITagGrp>() { grp };
        }

        var results = new List<ITagGrp>(); 
        foreach(var kvp in grp.Children)
        {
            var child = kvp.Value;
            if (child is TagUnion.TagGrp unionTagGroup)
            {
                var g = unionTagGroup.Value;
                if (g.IsEnabled)
                {
                    results.Add(g);
                }
                else
                {
                    var list = g.ScanEntries();
                    results.AddRange(list);
                }
            }
            // todo: support TagCbnt as Entry
        }
        return results;
    }
    #endregion

    #region
    /// <summary>
    /// 冒泡式获取通信通道
    /// </summary>
    /// <param name="tagGrp"></param>
    /// <returns></returns>
    public static ITagChannel? GetChannel(this ITagGrp tagGrp)
    {
        if(tagGrp.Channel is not null)
        {
            return tagGrp.Channel;
        }

        if(tagGrp.Parent is not null)
        {
            return tagGrp.Parent.GetChannel();
        }

        return null;
    }


    /// <summary>
    /// 冒泡式获取通信通道，如果为空则抛出异常
    /// </summary>
    /// <param name="tagGrp"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static ITagChannel GetRequiredChannel(this ITagGrp tagGrp) => tagGrp.GetChannel() ?? throw new Exception($"Channel is not configured : TagGrp({tagGrp.Name})");
    #endregion
}