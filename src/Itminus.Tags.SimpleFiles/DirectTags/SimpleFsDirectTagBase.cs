
namespace Itminus.Tags.SimpleFiles;

internal abstract class SimpleFsDirectTagBase<T> : Tag<T, SimpleFilesTagChannel>
{

    private bool? _autoCreateFile;

    protected SimpleFsDirectTagBase(TagDescriptor descriptor, SimpleFilesTagChannel? thisChannel, TagContainer container) 
        : base(descriptor, thisChannel, container)
    {
    }

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

    protected abstract T ParseValue(string text);


    protected virtual async Task CreateAndWriteDefaultAsync(string path, CancellationToken ct)
    {
        var data = $"{default(byte)}";
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
        var value = this._value?.ToString();
        await File.WriteAllTextAsync(path, value, ct);
        this.NotifyTagWritten(value);
        this.IsDirty = false;
    }
}



