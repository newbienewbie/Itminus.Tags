
namespace Itminus.Tags.SimpleFiles;

/// <summary>
/// SimpleFiles 直接测点基类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SimpleFilesDirectTagBase<T> : Tag<T, SimpleFilesTagChannel>
{


    /// <summary>
    /// c'tor
    /// </summary>
    /// <param name="descriptor"></param>
    /// <param name="thisChannel"></param>
    /// <param name="container"></param>
    protected SimpleFilesDirectTagBase(TagDescriptor descriptor, SimpleFilesTagChannel? thisChannel, TagContainer container) 
        : base(descriptor, thisChannel, container)
    {
    }

    private bool? _autoCreateFile;

    /// <summary>
    /// 是否自动创建文件
    /// </summary>
    public virtual bool AutoCreateFile
    {
        get
        {
            if (this._autoCreateFile.HasValue)
            {
                return this._autoCreateFile.Value;
            }

            if (!this.TagDescriptor.Extras.TryGetValue("AutoCreateFile", out var autoCreateFileValue))
            {
                this._autoCreateFile = false;
                return false;
            }

            if (!bool.TryParse(autoCreateFileValue.Value, out var autoCreateFile))
            {
                throw new Exception($"测点({this.TagName()})配置了AutoCreateFile，但无法解析为布尔值：{autoCreateFileValue}");
            }

            this._autoCreateFile = autoCreateFile;
            return autoCreateFile;
        }
    }

    /// <inheritdoc/>
    public override async Task ReadAsync(CancellationToken ct)
    {
        var path = this.NormalizedAddress();
        if (!File.Exists(path))
        {
            if (this.AutoCreateFile)
            {
                await CreateAndWriteDefaultAsync(path, ct);
            }
            return;
        }
        var text = await File.ReadAllTextAsync(path, ct);
        var value = this.ParseValue(text);
        this._value = value;
        this.Timestamp = DateTime.UtcNow;
        this.NotifyTagRead(value);
    }

    /// <summary>
    /// 解析文件内容为测点值
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    protected abstract T ParseValue(string text);

    /// <summary>
    /// 将测点值格式化为文件内容。<br/>
    /// 与 <see cref="ParseValue"/> 对称：<see cref="ParseValue"/> 负责 文本→值，本方法负责 值→文本。<br/>
    /// 默认实现使用 <see cref="object.ToString()"/>，基元类型（int/float 等）无需重写；
    /// 自定义类型（如 JSON POCO）应重写本方法，并处理 <paramref name="value"/> 为 null 的情况（自动创建默认文件时会以 <c>default(T)</c> 调用）。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual string FormatValue(T? value)
    {
        return value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 创建文件并写入默认值。<br/>
    /// 默认内容由 <see cref="FormatValue"/> 生成（以 <c>default(T)</c> 调用）。
    /// </summary>
    /// <param name="path"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected virtual async Task CreateAndWriteDefaultAsync(string path, CancellationToken ct)
    {
        var data = this.FormatValue(default);
        await File.WriteAllTextAsync(path, data, ct);
        return;
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(CancellationToken ct)
    {
        var path = this.NormalizedAddress();
        if (!File.Exists(path))
        {
            if (!this.AutoCreateFile)
            {
                return;
            }
            // 自动创建文件后继续往下执行写入
            await this.CreateAndWriteDefaultAsync(path, ct);
        }
        var value = this.FormatValue(this._value);
        await File.WriteAllTextAsync(path, value, ct);
        this.NotifyTagWritten(value);
        this.IsDirty = false;
    }
}



