using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Itminus.Tags;

public class TagSyncEventArgs : EventArgs 
{
    public TagSyncEventArgs(object? newValue, DateTime timestamp)
    {
        this.NewValue = newValue;
        this.Timestamp = timestamp;
    }

    public object? NewValue { get; set; }
    public DateTime Timestamp { get; set; }
}

public delegate void TagSyncEventHandler(ITag sender, TagSyncEventArgs e);

/// <summary>
/// 测点接口
/// </summary>
public interface ITag 
{
    /// <summary>
    /// 测点描述
    /// </summary>
    public TagDescriptor TagDescriptor{ get; set; }

    /// <summary>
    /// 测点值 
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp{ get; set; }

    /// <summary>
    /// 在每次从底层读取后，触发事件
    /// </summary>
    public event TagSyncEventHandler OnTagRead;

    /// <summary>
    /// 在每次向底层写入后，触发事件
    /// </summary>
    public event TagSyncEventHandler OnTagWritten;

    /// <summary>
    /// 是否被扫描过
    /// </summary>
    public bool IsScaned { get; set; }

    /// <summary>
    /// 标识数据是否发生变化，如果发生变化，会在输出阶段刷写到硬件底层
    /// </summary>
    public bool IsDirty { get; set; }

    /// <summary>
    /// 测点通道
    /// </summary>
    public ITagChannel? Channel { get; }

    /// <summary>
    /// 从底层中读取测点值
    /// </summary>
    /// <returns></returns>
    public Task ReadAsync(CancellationToken ct);

    /// <summary>
    /// 把当前测点值刷到底层
    /// </summary>
    /// <returns></returns>
    public Task WriteAsync(CancellationToken ct);
}

public static class ITagExtensions
{
    /// <summary>
    /// 测点名称
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static string TagName(this ITag tag) => tag.TagDescriptor.TagName;

    /// <summary>
    /// 测点地址
    /// </summary>
    public static TagAddress TagAddress(this ITag tag) => tag.TagDescriptor.Address;

    /// <summary>
    /// 所占据的字节多少
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static int TagSize(this ITag tag) => tag.TagDescriptor.TagSize;

    /// <summary>
    /// 测点种类
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static TagKinds TagKind(this ITag tag) => tag.TagDescriptor.TagKind;

    /// <summary>
    /// 大小端
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static EndianKinds TagEndian(this ITag tag) => tag.TagDescriptor.EndianKind;

    /// <summary>
    /// 访问模式
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static TagAccessMode AccessMode(this ITag tag) => tag.TagDescriptor.AccessMode;

    /// <summary>
    /// 把当前测点转成具体类型
    /// </summary>
    /// <typeparam name="TTag"></typeparam>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static TTag AsTag<TTag>(this ITag tag) where TTag : ITag
    {
        if (tag is not TTag t)
        {
            throw new Exception($"{tag.GetType()} is not {typeof(TTag)}");
        }
        return t;
    }


    /// <summary>
    /// 获取当前测点的值
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static TValue? GetTagValue<TValue>(this ITag tag)
    {
        TValue value = (TValue)tag.Value!;
        return value;
    }
}