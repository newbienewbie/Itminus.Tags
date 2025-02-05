using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags;

/// <summary>
/// 测点组合配置
/// </summary>
/// <typeparam name="TChannel"></typeparam>
public abstract class TagsCbntConfiguration
{
    /// <summary>
    /// 测点组合名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 起始地址
    /// </summary>
    public string StartAddress { get; set; } = string.Empty;

    /// <summary>
    /// 间隔
    /// </summary>
    public int ScanInterval { get; set; } = 20;

    /// <summary>
    /// 测点描述
    /// </summary>
    public IList<TagDescriptorSetting> Tags { get; set; } = null!;

    /// <summary>
    /// 测点描述
    /// </summary>
    public IList<TagDescriptor> TagDescriptors => Tags
        .Select(t => new TagDescriptor {
            TagKind = Enum.Parse<TagKinds>(t.TagKind),
            TagSize = t.TagSize,
            TagName = t.TagName,
            Address = t.Address,
            EndianKind = t.EndianKind,
            Note = t.Note,
        })
        .ToList();
}


/// <summary>
/// 测点描述
/// </summary>
public class TagDescriptorSetting
{
    /// <summary>
    /// 测点名称——用于展示，在测组中具有唯一性
    /// </summary>
    public string TagName { set; get; } = null!;

    /// <summary>
    /// 测点类型，位、字节、ushort、整型、浮点等
    /// </summary>
    public string TagKind { set; get; } = string.Empty;

    public int TagSize { get; set; }

    /// <summary>
    /// 测点地址
    /// </summary>
    public TagAddress Address { set; get; } = null!;

    /// <summary>
    /// 大小尾
    /// </summary>
    public EndianKinds EndianKind { set; get; } = EndianKinds.LittleEndian;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Note { set; get; }
}
