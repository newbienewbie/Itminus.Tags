using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Itminus.Tags.McpServer;

/// <summary>
/// 基于 Itminus.Tags 的 MCP 工具集，提供对已运行项目的测点读写操作。
/// </summary>
[McpServerToolType]
public class TagsMcpServerTools
{
    private readonly ITagsProjectCtrl _ctrl;
    private readonly ILogger<TagsMcpServerTools> _logger;

    /// <summary>创建 <see cref="TagsMcpServerTools"/> 实例。</summary>
    public TagsMcpServerTools(ITagsProjectCtrl ctrl, ILogger<TagsMcpServerTools> logger)
    {
        _ctrl = ctrl;
        _logger = logger;
    }

    #region
    /// <summary>
    /// 列出当前运行项目静态描述信息，返回一段 XML，其中描述了各个通道、层级式的测点点位。
    /// 这里列出的描述信息，为后续所有操作提供了必要上下文信息。
    /// 你总是应该先调用此方法，获取项目的测点树静态结构，然后再进行测点读写操作。
    /// 这是因为读写时需要提供测点路径，而路径来自于这里项目静态XML描述——用各级元素的"name"形成一个路径，以 "/" 分隔层级，
    /// <example>
    /// 如 "IoBox/通用状态/PLC/心跳请求"， 
    /// 表示: `&lt;TagGrp name='IoBox'/ &gt;`下有一个`&lt;TagGrp name='通用状态' &gt;`元素，
    /// 其下又有一个`name='PLC'的`TagGrp`或者`TagCnbt`元素，
    /// 最后又嵌套了一个`name='心跳请求'`的`Tag`节点。
    /// </example>
    /// 读写测点时，必须使用完整路径。
    /// </summary>
    /// <returns></returns>
    [McpServerTool]
    public string ListProjectTree()
    {
        var root = this._ctrl.Project?.RootElement;
        return root?.ToString() ?? "there's no project yet. You should start it before you go on";
    }
    #endregion


    #region  读取测点
    /// <summary>
    /// 按完整路径读取单个测点的值，同时返回元数据（类型、访问模式、时间戳等）。
    /// </summary>
    /// <param name="path">测点的完整路径，路径来自项目静态描述文件，用各级元素的"name"形成一个路径，以 "/" 分隔层级，如 "IoBox/通用状态/PLC/心跳请求"。</param>
    [McpServerTool]
    public TagValue ReadTagValue(string path)
    {
        var proj = EnsureProject();
        ITag tag;
        try { tag = proj.Tags.SelectTag(path); }
        catch (Exception ex) { throw new InvalidOperationException($"Failed to find tag at path '{path}': {ex.Message}"); }
        return BuildTagValue(tag);
    }

    /// <summary>
    /// 按完整路径批量读取多个测点的值。
    /// </summary>
    /// <param name="paths">测点的完整路径数组，每个路径以 "/" 分隔层级。</param>
    [McpServerTool]
    public List<TagValue> ReadTagValues(string[] paths)
    {
        var proj = EnsureProject();
        var results = new List<TagValue>();
        foreach (var path in paths)
        {
            try
            {
                var tag = proj.Tags.SelectTag(path);
                results.Add(BuildTagValue(tag));
            }
            catch (Exception ex)
            {
                results.Add(new TagValue(null, null, DateTime.MinValue, ex.Message));
            }
        }
        return results;
    }

#endregion

#region  写入测点
    /// <summary>
    /// 按完整路径写入单个测点的值，会根据 TagKind 自动转换类型。
    /// </summary>
    /// <param name="path">测点的完整路径。</param>
    /// <param name="value">要写入的值，需要与测点类型兼容（BIT→bool，INT16→short，STR→string 等）。</param>
    /// <param name="waitForCompletion">是否等待写入完成，默认为 true。</param>
    [McpServerTool]
    public async Task<WriteResult> WriteTagValue(string path, object? value, bool waitForCompletion = true)
    {
        var proj = EnsureProject();

        ITag tag;
        try { tag = proj.Tags.SelectTag(path); }
        catch (Exception ex) { return WriteResult.Fail($"Failed to find tag at path '{path}': {ex.Message}"); }

        if (tag.AccessMode() == TagAccessMode.RO)
            return WriteResult.Fail($"Tag '{path}' is read-only (access mode: RO). Cannot write.");

        var entry = tag.SearchEntry();
        if (entry is null)
            return WriteResult.Fail($"Could not find an entry group containing tag '{path}'.");

        var convertedValue = ConvertValue(value, tag.TagKind());

        TagGrpWriteIntent intent = (_, _) =>
        {
            tag.Value = convertedValue;
            return ValueTask.CompletedTask;
        };

        if (!proj.WriteIntent(entry.Name, intent, out var task))
            return WriteResult.Fail($"Failed to write intent for tag '{path}' in entry '{entry.Name}'. Intent queue may be full.");

        if (waitForCompletion)
        {
            try
            {
                await task;
                return WriteResult.Ok($"Successfully wrote tag '{path}' with value: {convertedValue}");
            }
            catch (Exception ex) { return WriteResult.Fail($"Write intent for tag '{path}' failed: {ex.Message}"); }
        }

        return WriteResult.Ok($"Write intent for tag '{path}' queued successfully (not waiting for completion).");
    }

    /// <summary>
    /// 按完整路径批量写入多个测点的值。
    /// 内部按入口组（entry）分组，每个入口组合并为一个 WriteIntent，以提高效率。
    /// </summary>
    /// <param name="tagValues">字典，key 为测点完整路径，value 为要写入的值。</param>
    /// <param name="waitForCompletion">是否等待所有写入意图完成，默认为 true。</param>
    [McpServerTool]
    public async Task<WriteResult> WriteTagValues(Dictionary<string, object?> tagValues, bool waitForCompletion = true)
    {
        var proj = EnsureProject();

        // Phase 1: resolve all tags and group by entry
        var perEntry = new Dictionary<string, List<(string TagPath, ITag Tag, object? ConvertedValue)>>();
        foreach (var kvp in tagValues)
        {
            var path = kvp.Key;
            ITag tag;
            try { tag = proj.Tags.SelectTag(path); }
            catch (Exception ex) { return WriteResult.Fail($"Failed to find tag at '{path}': {ex.Message}"); }

            if (tag.AccessMode() == TagAccessMode.RO)
                return WriteResult.Fail($"Tag '{path}' is read-only. Aborting batch write.");

            var entry = tag.SearchEntry();
            if (entry is null)
                return WriteResult.Fail($"Could not find an entry group containing tag '{path}'.");

            var convertedValue = ConvertValue(kvp.Value, tag.TagKind());

            if (!perEntry.TryGetValue(entry.Name, out var list))
            {
                list = new List<(string, ITag, object?)>();
                perEntry[entry.Name] = list;
            }
            list.Add((path, tag, convertedValue));
        }

        // Phase 2: issue one intent per entry, collect tasks
        var tasks = new List<Task>();
        var results = new List<string>();
        foreach (var (entryName, writes) in perEntry)
        {
            var tagsToWrite = writes.Select(w => (w.Tag, w.ConvertedValue)).ToList();
            TagGrpWriteIntent intent = (_, _) =>
            {
                foreach (var (t, cv) in tagsToWrite)
                    t.Value = cv;
                return ValueTask.CompletedTask;
            };

            if (!proj.WriteIntent(entryName, intent, out var task))
                return WriteResult.Fail($"Failed to write batch intent for entry '{entryName}'. Intent queue may be full.");

            tasks.Add(task);
            foreach (var (tagPath, _, cv) in writes)
                results.Add($"{tagPath}={cv}");
        }

        if (waitForCompletion)
        {
            try
            {
                await Task.WhenAll(tasks);
                return WriteResult.Ok($"Successfully wrote {results.Count} tag(s) across {perEntry.Count} entry group(s): {string.Join(", ", results)}");
            }
            catch (Exception ex) { return WriteResult.Fail($"Batch write failed: {ex.Message}"); }
        }

        return WriteResult.Ok($"Write intents for {results.Count} tag(s) across {perEntry.Count} entry group(s) queued successfully.");
    }
#endregion



#region 内部辅助函数
    private ITagsProject EnsureProject() =>
        _ctrl.Project ?? throw new InvalidOperationException("No project is running.");

    private static TagValue BuildTagValue(ITag tag)
    {
        try
        {
            return new TagValue(tag.TagDescriptor, tag.Value, tag.Timestamp);
        }
        catch (Exception ex)
        {
            return new TagValue(tag.TagDescriptor, null, tag.Timestamp, ex.Message);
        }
    }

    private static object? ConvertValue(object? value, TagKinds tagKind)
    {
        if (value is null) return null;

        return tagKind switch
        {
            BuiltinTagKinds.BIT => value is bool b ? b : Convert.ToBoolean(value),
            BuiltinTagKinds.BYTE => value is byte by ? by : Convert.ToByte(value),
            BuiltinTagKinds.INT16 => value is short s ? s : Convert.ToInt16(value),
            BuiltinTagKinds.UINT16 => value is ushort us ? us : Convert.ToUInt16(value),
            BuiltinTagKinds.INT32 => value is int i ? i : Convert.ToInt32(value),
            BuiltinTagKinds.UINT32 => value is uint ui ? ui : Convert.ToUInt32(value),
            BuiltinTagKinds.INT64 => value is long l ? l : Convert.ToInt64(value),
            BuiltinTagKinds.UINT64 => value is ulong ul ? ul : Convert.ToUInt64(value),
            BuiltinTagKinds.FLOAT => value is float f ? f : Convert.ToSingle(value),
            BuiltinTagKinds.STR => value?.ToString() ?? "",
            BuiltinTagKinds.DI => value is bool b2 ? b2 : Convert.ToBoolean(value),
            BuiltinTagKinds.DO => value is bool b3 ? b3 : Convert.ToBoolean(value),
            _ => value
        };
    }

#endregion
}


#region 结果
/// <summary>测点读取结果。</summary>
/// <param name="Descriptor">测点元数据（名称、类型、地址、大小、访问模式等）。测点未找到时为 null。</param>
/// <param name="Value">当前测点值。</param>
/// <param name="Timestamp">最后一次读取的时间戳。</param>
/// <param name="Error">读取失败时的错误信息。</param>
public record TagValue(TagDescriptor? Descriptor, object? Value, DateTime Timestamp, string? Error = null);

/// <summary>写入操作结果。</summary>
/// <param name="Success">是否写入成功。</param>
/// <param name="Message">描述信息。</param>
public record WriteResult(bool Success, string Message)
{
    /// <summary>创建成功结果。</summary>
    public static WriteResult Ok(string message) => new(true, message);

    /// <summary>创建失败结果。</summary>
    public static WriteResult Fail(string message) => new(false, message);
}
#endregion

