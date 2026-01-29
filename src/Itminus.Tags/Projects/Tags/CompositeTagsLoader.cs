using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Itminus.Tags.Projects;

/// <summary>
/// 根据channel和element，给出 <see cref="TagCbntBuilderBase"/> <br/>。
/// 如果当前参数不合适，给出null。
/// </summary>
/// <param name="channel"></param>
/// <param name="element"></param>
/// <returns></returns>
public delegate TagCbntBuilderBase? MakeTagCbntBuilder(ITagChannel channel, XElement element);

/// <summary>
/// 根据channel、tagDescriptor 和element，给出<see cref="TagBuilderBase"/>  <br/>
/// 如果当前参数不合适，给出null。
/// </summary>
/// <param name="channel"></param>
/// <param name="descriptor"></param>
/// <param name="element"></param>
/// <returns></returns>
public delegate TagBuilderBase? MakeTagBuilder(ITagChannel channel, TagDescriptor descriptor, XElement element);

/// <summary>
/// 复合测点集加载器。<br/>
/// </summary>
public class CompositeTagsLoader : ITagsLoader
{
    #region TagsBuilder Choose
    /// <summary>
    /// 支持的测点构建器集合
    /// </summary>
    protected List<MakeTagBuilder> _tagFactories = new();

    /// <summary>
    /// 注册 <see cref="TagBuilder"/>的构建器
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public virtual CompositeTagsLoader AddTagBuilder(MakeTagBuilder factory)
    {
        this._tagFactories.Add(factory);
        return this;
    }

    /// <summary>
    /// 根据给定的channel和element, 生成合适的 <see cref="TagBuilderBase"/> <br/>
    /// 返回null表示未找到结果
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="thisElement"></param>
    /// <returns></returns>
    protected virtual TagBuilderBase? ChooseTagBuilder(ITagChannel channel, TagDescriptor tagDescriptor, XElement thisElement)
    {
        foreach (var f in this._tagFactories)
        {
            var x = f(channel, tagDescriptor, thisElement);
            if (x != null)
            {
                return x;
            }
        }
        return null;
    }
    #endregion


    #region TagsCbntBulder Choice
    /// <summary>
    /// 支持的测点构建器集合
    /// </summary>
    protected List<MakeTagCbntBuilder> _tagCbntBuilders = new();

    /// <summary>
    /// 添加测点组合构建器
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public virtual CompositeTagsLoader AddTagsCbntBuilder(MakeTagCbntBuilder func)
    {
        this._tagCbntBuilders.Add(func);
        return this;
    }

    /// <summary>
    /// 按顺序，逐一调用测点组合构建器，如果返回为null，表示当前构建器不适用于对应的节点，需要继续尝试其它构建器
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="thisElement"></param>
    /// <returns></returns>
    protected virtual TagCbntBuilderBase? ChooseTagCbntBuilder(ITagChannel channel, XElement thisElement)
    {
        foreach (var b in this._tagCbntBuilders)
        {
            var x = b(channel, thisElement);
            if (x != null)
            {
                return x;
            }
        }
        return null;
    }
    #endregion


    #region 从 XElement 中加载 Tag|TagCbnt|TagGrp，并作为子节点追加到指定的父节点中
    /// <inheritdoc/>
    public virtual void LoadTagGroup(ITagGrp parent, XElement thisElement, IList<ITagChannel> availableChannels)
    {
        var thisTagName = thisElement.GetTagUnionName();
        var thisIsEntry = thisElement.GetTagUnionIsEntry(thisTagName);
        var thisChannel = thisElement.GetTagUnionChannel(availableChannels);

        var unit = new ValueTuple();
        thisElement.MapTagUnion(
            e =>
            {
                var channel = thisChannel ?? parent.GetRequiredChannel();
                var descriptor = this.LoadTagDescriptor(thisElement);
                var builder = this.ChooseTagBuilder(channel, descriptor, thisElement) ??
                    throw new NotImplementedException($"未注册相应的 TagBuilder: 通道（Name={channel.ChannelName}, Driver={channel.Driver}), Element={thisElement.ToString()}");
                var tag = builder.Build();
                parent.AddTag(tag);
                return unit;
            },
            e =>
            {
                var channel = thisChannel ?? parent.GetRequiredChannel();
                var builder = this.ChooseTagCbntBuilder(channel, thisElement) ??
                    throw new Exception($"未注册相应的TagCbntBuilder: 通道（Name={channel.ChannelName}, Driver={channel.Driver}), Element={thisElement.ToString()}");
                var cbntors = e.Elements().Select(t => LoadTagDescriptor(t)).ToList();
                var cbntBuilder = builder
                    .AddTags(cbntors);
                var accessMode = e.GetTagUnionAccess(thisTagName);
                if (accessMode.HasValue)
                {
                    cbntBuilder.WithAccessMode(accessMode.Value);
                }
                var cbnt = cbntBuilder.Build();
                parent.AddTag(cbnt);
                return unit;
            },
            e =>
            {
                var thisGrp = new TagGrp(thisTagName, thisIsEntry, thisChannel);
                var isEnabled = !string.Equals(thisElement.Attribute("isEnabled")?.Value, "false", StringComparison.OrdinalIgnoreCase);
                thisGrp.IsEnabled = isEnabled;
                thisGrp.ScanInterval = thisElement.GetTagUnionScanInterval(thisGrp.Name) ?? (parent.GetScanInterval() ?? 1000);
                parent.AddTag(thisGrp);
                foreach (var childElement in thisElement.Elements())
                {
                    LoadTagGroup(thisGrp, childElement, availableChannels);
                }
                return unit;
            }
        );
    }


    protected virtual TagDescriptor LoadTagDescriptor(XElement e)
    {
        var tagName = e.GetTagUnionName();
        var address = e.GetTagUnionAddress(tagName);
        var tagKind = e.GetTagUnionTagKind(tagName);
        var tagEndian = e.GetTagUnionEndian(tagName);

        var tagNote = e.GetTagUnionNote(tagName);

        var tagdescriptor = new TagDescriptor()
        {
            Address = address,
            TagName = tagName,
            TagKind = tagKind,
            EndianKind = tagEndian,
            Note = tagNote,
        };

        var tagAccess = e.GetTagUnionAccess(tagName);
        if (tagAccess.HasValue)
        {
            tagdescriptor.AccessMode = tagAccess.Value;
        }

        var tagSize = (string?)e.Attribute("tagSize");
        if (!string.IsNullOrEmpty(tagSize))
        {
            if (!int.TryParse(tagSize, out var size))
            {
                throw new Exception($"Tag(Name={tagName}) 配置了非法大小={tagSize}");
            }
            tagdescriptor.TagSize = size;
        }
        return tagdescriptor;
    }
    #endregion

}
