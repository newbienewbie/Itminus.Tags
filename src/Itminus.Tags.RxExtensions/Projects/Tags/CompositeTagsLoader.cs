using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Itminus.Tags.Projects;

/// <summary>
/// 复合测点集加载器。<br/>
/// 会按顺序逐一调用内部测点构建器集合，如果某个构建器返回为null，表示当前构建器不适用于对应的节点，需要继续尝试其它构建器。
/// </summary>
public class CompositeTagsLoader : ITagsLoader
{

    #region TagsCbntBulder Choice
    /// <summary>
    /// 支持的测点构建器集合
    /// </summary>
    protected List<Func<ITagChannel, XElement, TagCbntBuilderBase?>> _tagCbntBuilders = new();

    /// <summary>
    /// 添加测点组合构建器
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public virtual CompositeTagsLoader AddTagsCbntBuilder(Func<ITagChannel, XElement, TagCbntBuilderBase?> func)
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




    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual ITagGrp LoadTagGroups(string indexPath, IList<ITagChannel> channels)
    {
        var files = this.ParseTagsIndex(indexPath);
        var root = new TagGrp("root", false, null);
        foreach (var file in files)
        {
            LoadTagGroups(root, file, channels);
        }
        return root;
    }


    private IList<string> ParseTagsIndex(string indexPath)
    {
        if (!File.Exists(indexPath))
        {
            throw new Exception($"指定的测点索引文件路径不存在({indexPath})");
        }
        var dir = Path.GetDirectoryName(indexPath) ?? throw new Exception("无法获取测点索引所在目录");
        var doc = XDocument.Load(indexPath);
        var list = doc.Root?.Elements("file")
            .Select(e => ((string?)e.Attribute("path")))
            .Where(e => e != null)
            .Select(e => Path.Combine(dir, e!))
            .ToList();
        return list ?? new List<string>();
    }


    protected virtual ITagGrp LoadTagGroups(TagGrp rootGrp, string path, IList<ITagChannel> channels)
    {
        var doc = XDocument.Load(path);
        var thisElement = doc.Root!;
        LoadTagGroups(rootGrp, thisElement, channels);
        return rootGrp;
    }


    protected virtual void LoadTagGroups(ITagGrp parent, XElement thisElement, IList<ITagChannel> availableChannels)
    {
        var thisTagName = thisElement.GetTagUnionName();
        var thisIsEntry = thisElement.GetTagUnionIsEntry(thisTagName);
        var thisChannel = thisElement.GetTagUnionChannel(availableChannels);

        thisElement.MapTagUnion(
            e =>
            {
                throw new Exception();
            },
            e =>
            {
                var channel = thisChannel ?? parent.GetRequiredChannel();
                var builder = this.ChooseTagCbntBuilder(channel, thisElement) ??
                    throw new Exception($"未注册相应的TagCbntBuilder: 通道（Name={channel.ChannelName}, Driver={channel.Driver}), Element={thisElement.ToString()}");
                var cbntors = e.Elements().Select(t => LoadTagCbntor(t)).ToList();
                var cbntBuilder = builder
                    .AddTags(cbntors);
                var accessMode = e.GetTagUnionAccess(thisTagName);
                if (accessMode.HasValue)
                {
                    cbntBuilder.WithAccessMode(accessMode.Value);
                }
                var cbnt = cbntBuilder.Build();
                parent.AddTag(cbnt);
                return Unit.Default;
            },
            e =>
            {
                var thisGrp = new TagGrp(thisTagName, thisIsEntry, thisChannel);
                parent.AddTag(thisGrp);
                foreach (var childElement in thisElement.Elements())
                {
                    LoadTagGroups(thisGrp, childElement, availableChannels);
                }
                return Unit.Default;
            }
        );
    }


    protected virtual TagDescriptor LoadTagCbntor(XElement e)
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


}
