using System.Globalization;

namespace NixMonitor.Tags.SimpleTags;

/// <summary>
/// Linux <c>/proc/meminfo</c> 的解析模型。<br/>
/// 所有字段统一以 <b>字节(Bytes)</b> 为单位（源文件为 kB，解析时已换算）。<br/>
/// 字段与树莓派 /proc/meminfo 输出一一对应，缺失字段（不同内核版本可能不同）取 0。<br/>
/// 本类型为 <see langword="record"/>：全部字段均为值类型，值相等性即字段完全相等。
/// </summary>
public sealed record MemInfo
{
    public long MemTotal { get; init; }
    public long MemFree { get; init; }
    public long MemAvailable { get; init; }
    public long Buffers { get; init; }
    public long Cached { get; init; }
    public long SwapCached { get; init; }
    public long Active { get; init; }
    public long Inactive { get; init; }
    public long ActiveAnon { get; init; }
    public long InactiveAnon { get; init; }
    public long ActiveFile { get; init; }
    public long InactiveFile { get; init; }
    public long Unevictable { get; init; }
    public long Mlocked { get; init; }
    public long SwapTotal { get; init; }
    public long SwapFree { get; init; }
    public long Zswap { get; init; }
    public long Zswapped { get; init; }
    public long Dirty { get; init; }
    public long Writeback { get; init; }
    public long AnonPages { get; init; }
    public long Mapped { get; init; }
    public long Shmem { get; init; }
    public long KReclaimable { get; init; }
    public long Slab { get; init; }
    public long SReclaimable { get; init; }
    public long SUnreclaim { get; init; }
    public long KernelStack { get; init; }
    public long PageTables { get; init; }
    public long SecPageTables { get; init; }
    public long NfsUnstable { get; init; }
    public long Bounce { get; init; }
    public long WritebackTmp { get; init; }
    public long CommitLimit { get; init; }
    public long CommittedAs { get; init; }
    public long VmallocTotal { get; init; }
    public long VmallocUsed { get; init; }
    public long VmallocChunk { get; init; }
    public long Percpu { get; init; }
    public long CmaTotal { get; init; }
    public long CmaFree { get; init; }

    /// <summary>
    /// 解析 <c>/proc/meminfo</c> 文本。<br/>
    /// 每行格式：<c>Key: 数值 单位</c>，单位 kB 统一换算为字节。
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static MemInfo Parse(string text)
    {
        var values = new Dictionary<string, long>(StringComparer.Ordinal);

        foreach (var line in text.Split('\n'))
        {
            var colon = line.IndexOf(':');
            if (colon < 0) { continue; }

            var key = line[..colon].Trim();
            if (key.Length == 0) { continue; }

            var rest = line[(colon + 1)..].Trim();
            var parts = rest.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || !long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
            {
                continue;
            }

            var unit = parts.Length > 1 ? parts[1] : string.Empty;
            values[key] = unit switch
            {
                "B" => num,
                "kB" => num * 1024L,
                "MB" => num * 1024L * 1024L,
                "GB" => num * 1024L * 1024L * 1024L,
                _ => num,
            };
        }

        long Get(string key) => values.TryGetValue(key, out var v) ? v : 0L;

        return new MemInfo
        {
            MemTotal = Get("MemTotal"),
            MemFree = Get("MemFree"),
            MemAvailable = Get("MemAvailable"),
            Buffers = Get("Buffers"),
            Cached = Get("Cached"),
            SwapCached = Get("SwapCached"),
            Active = Get("Active"),
            Inactive = Get("Inactive"),
            ActiveAnon = Get("Active(anon)"),
            InactiveAnon = Get("Inactive(anon)"),
            ActiveFile = Get("Active(file)"),
            InactiveFile = Get("Inactive(file)"),
            Unevictable = Get("Unevictable"),
            Mlocked = Get("Mlocked"),
            SwapTotal = Get("SwapTotal"),
            SwapFree = Get("SwapFree"),
            Zswap = Get("Zswap"),
            Zswapped = Get("Zswapped"),
            Dirty = Get("Dirty"),
            Writeback = Get("Writeback"),
            AnonPages = Get("AnonPages"),
            Mapped = Get("Mapped"),
            Shmem = Get("Shmem"),
            KReclaimable = Get("KReclaimable"),
            Slab = Get("Slab"),
            SReclaimable = Get("SReclaimable"),
            SUnreclaim = Get("SUnreclaim"),
            KernelStack = Get("KernelStack"),
            PageTables = Get("PageTables"),
            SecPageTables = Get("SecPageTables"),
            NfsUnstable = Get("NFS_Unstable"),
            Bounce = Get("Bounce"),
            WritebackTmp = Get("WritebackTmp"),
            CommitLimit = Get("CommitLimit"),
            CommittedAs = Get("Committed_AS"),
            VmallocTotal = Get("VmallocTotal"),
            VmallocUsed = Get("VmallocUsed"),
            VmallocChunk = Get("VmallocChunk"),
            Percpu = Get("Percpu"),
            CmaTotal = Get("CmaTotal"),
            CmaFree = Get("CmaFree"),
        };
    }

    /// <summary>
    /// 返回人类可读的内存摘要（MB/GB 单位 + 内存使用率）
    /// </summary>
    public override string ToString()
    {
        var usedPercent = this.MemTotal > 0
            ? (this.MemTotal - this.MemAvailable) * 100.0 / this.MemTotal
            : 0;
        return $"MemTotal={FormatBytes(this.MemTotal)} MemFree={FormatBytes(this.MemFree)} " +
               $"MemAvailable={FormatBytes(this.MemAvailable)} Used={usedPercent:F1}%";
    }

    /// <summary>
    /// 将字节数格式化为人类可读的 MB/GB 字符串
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        const double KB = 1024d, MB = KB * 1024d, GB = MB * 1024d;
        return bytes >= GB ? $"{bytes / GB:F2}GB" : $"{bytes / MB:F1}MB";
    }
}

/// <summary>
/// Linux <c>/proc/cpuinfo</c> 单个处理器（processor 块）的解析模型。<br/>
/// 注意：<see cref="Others"/> 为字典，record 合成的值相等性对它是<b>引用比较</b>，
/// 如需严格值相等请自定义 <see cref="object.Equals(object?)"/>。
/// </summary>
public sealed record CpuProcessorInfo
{
    /// <summary>
    /// 处理器编号（<c>processor</c> 字段）
    /// </summary>
    public int Processor { get; init; }

    /// <summary>
    /// 型号名称（<c>model name</c> 字段，x86/ARM 常见）
    /// </summary>
    public string? ModelName { get; init; }

    /// <summary>
    /// 厂商（<c>vendor_id</c> 字段，x86）
    /// </summary>
    public string? VendorId { get; init; }

    /// <summary>
    /// BogoMIPS（<c>BogoMIPS</c> 字段，ARM 常见）
    /// </summary>
    public double? BogoMips { get; init; }

    /// <summary>
    /// 特性列表（ARM 的 <c>Features</c> 或 x86 的 <c>flags</c>）
    /// </summary>
    public string? Features { get; init; }

    /// <summary>
    /// 硬件型号（ARM 的 <c>Hardware</c>）
    /// </summary>
    public string? Hardware { get; init; }

    /// <summary>
    /// 硬件修订号（ARM 的 <c>Revision</c>）
    /// </summary>
    public string? Revision { get; init; }

    /// <summary>
    /// 序列号（ARM 的 <c>Serial</c>）
    /// </summary>
    public string? Serial { get; init; }

    /// <summary>
    /// 型号（ARM 的 <c>Model</c>）
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// 其它未枚举的字段（如 <c>cpu MHz</c>、<c>cache size</c> 等），键为源文件原始字段名
    /// </summary>
    public IReadOnlyDictionary<string, string> Others { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// 从单个 processor 块的键值对构造模型
    /// </summary>
    internal static CpuProcessorInfo FromDict(IReadOnlyDictionary<string, string> d)
    {
        static string? Get(IReadOnlyDictionary<string, string> d, string key)
            => d.TryGetValue(key, out var v) ? v : null;

        var known = new HashSet<string>(StringComparer.Ordinal)
        {
            "processor", "model name", "vendor_id", "BogoMIPS", "Features", "flags",
            "Hardware", "Revision", "Serial", "Model",
        };

        var others = d
            .Where(kv => !known.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

        return new CpuProcessorInfo
        {
            Processor = int.TryParse(Get(d, "processor"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var p) ? p : -1,
            ModelName = Get(d, "model name"),
            VendorId = Get(d, "vendor_id"),
            BogoMips = double.TryParse(Get(d, "BogoMIPS"), NumberStyles.Float, CultureInfo.InvariantCulture, out var b) ? b : null,
            Features = Get(d, "Features") ?? Get(d, "flags"),
            Hardware = Get(d, "Hardware"),
            Revision = Get(d, "Revision"),
            Serial = Get(d, "Serial"),
            Model = Get(d, "Model"),
            Others = others,
        };
    }

    /// <summary>
    /// 返回人类可读的处理器摘要
    /// </summary>
    public override string ToString()
    {
        var model = this.ModelName ?? this.Hardware ?? this.Model ?? "(unknown)";
        var bogo = this.BogoMips is { } b
            ? $" {b.ToString("F1", CultureInfo.InvariantCulture)} BogoMIPS"
            : string.Empty;
        return $"CPU#{this.Processor} {model}{bogo}";
    }
}

/// <summary>
/// Linux <c>/proc/cpuinfo</c> 的解析模型。<br/>
/// 多核系统包含多个 processor 块；<c>Hardware</c>/<c>Revision</c>/<c>Serial</c>/<c>Model</c> 等公共字段取第一个 processor 块的值。<br/>
/// 注意：<see cref="Processors"/> 为列表，record 合成的值相等性对它是<b>引用比较</b>，
/// 如需严格值相等请自定义 <see cref="object.Equals(object?)"/>。
/// </summary>
public sealed record CpuInfo
{
    /// <summary>
    /// 各处理器的信息
    /// </summary>
    public IReadOnlyList<CpuProcessorInfo> Processors { get; init; } = Array.Empty<CpuProcessorInfo>();

    /// <summary>
    /// 逻辑核心数
    /// </summary>
    public int CoreCount => Processors.Count;

    /// <summary>
    /// 硬件型号（如 <c>BCM2835</c>）
    /// </summary>
    public string? Hardware { get; init; }

    /// <summary>
    /// 硬件修订号（如 <c>a020d3</c>）
    /// </summary>
    public string? Revision { get; init; }

    /// <summary>
    /// 序列号
    /// </summary>
    public string? Serial { get; init; }

    /// <summary>
    /// 型号（如 <c>Raspberry Pi 4 Model B Rev 1.2</c>）
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// 解析 <c>/proc/cpuinfo</c> 文本。<br/>
    /// 处理器块之间以空行分隔，块内为 <c>key: value</c> 形式的键值对。
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static CpuInfo Parse(string text)
    {
        var processors = new List<CpuProcessorInfo>();
        Dictionary<string, string>? current = null;

        void Flush()
        {
            if (current is null) { return; }
            processors.Add(CpuProcessorInfo.FromDict(current));
            current = null;
        }

        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                Flush();
                continue;
            }

            current ??= new Dictionary<string, string>(StringComparer.Ordinal);
            var colon = line.IndexOf(':');
            if (colon < 0) { continue; }

            var key = line[..colon].Trim();
            if (key.Length == 0) { continue; }

            current[key] = line[(colon + 1)..].Trim();
        }
        Flush();

        var first = processors.FirstOrDefault();
        return new CpuInfo
        {
            Processors = processors,
            Hardware = first?.Hardware,
            Revision = first?.Revision,
            Serial = first?.Serial,
            Model = first?.Model,
        };
    }

    /// <summary>
    /// 返回人类可读的 CPU 摘要（核心数 + 型号）
    /// </summary>
    public override string ToString()
    {
        var model = this.Model ?? this.Hardware ?? "(unknown)";
        var rev = this.Revision is { Length: > 0 } r ? $" Rev={r}" : string.Empty;
        return $"Cores={this.CoreCount} Model={model}{rev}";
    }
}

/// <summary>
/// Linux <c>/proc/loadavg</c> 的解析模型。<br/>
/// 格式：<c>1分钟负载 5分钟负载 15分钟负载 运行中/总任务数 最后PID</c>
/// </summary>
public sealed record LoadAvg
{
    /// <summary>
    /// 1 分钟平均负载
    /// </summary>
    public double Load1 { get; init; }

    /// <summary>
    /// 5 分钟平均负载
    /// </summary>
    public double Load5 { get; init; }

    /// <summary>
    /// 15 分钟平均负载
    /// </summary>
    public double Load15 { get; init; }

    /// <summary>
    /// 运行中的任务数
    /// </summary>
    public int RunningTasks { get; init; }

    /// <summary>
    /// 总任务数
    /// </summary>
    public int TotalTasks { get; init; }

    /// <summary>
    /// 最近创建的进程 PID
    /// </summary>
    public int LastPid { get; init; }

    /// <summary>
    /// 解析 <c>/proc/loadavg</c> 文本
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static LoadAvg Parse(string text)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4) { return new LoadAvg(); }

        var run = parts[3].Split('/');

        return new LoadAvg
        {
            Load1 = ParseDouble(parts[0]),
            Load5 = ParseDouble(parts[1]),
            Load15 = ParseDouble(parts[2]),
            RunningTasks = run.Length > 0 && int.TryParse(run[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ? r : 0,
            TotalTasks = run.Length > 1 && int.TryParse(run[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var t) ? t : 0,
            LastPid = parts.Length > 4 && int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pid) ? pid : 0,
        };

        static double ParseDouble(string s)
            => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0d;
    }

    /// <summary>
    /// 返回人类可读的负载摘要
    /// </summary>
    public override string ToString()
    {
        static string F(double v) => v.ToString("F2", CultureInfo.InvariantCulture);
        return $"Load1={F(this.Load1)} Load5={F(this.Load5)} Load15={F(this.Load15)} " +
               $"Tasks={this.RunningTasks}/{this.TotalTasks} LastPid={this.LastPid}";
    }
}
