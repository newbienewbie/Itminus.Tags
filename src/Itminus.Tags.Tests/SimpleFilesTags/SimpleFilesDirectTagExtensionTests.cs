using Itminus.Tags;
using Itminus.Tags.SimpleFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Itminus.Tags.Tests.SimpleFilesTags;

/// <summary>
/// 验证外部可以：
/// 1. 继承 <see cref="SimpleFilesDirectTagBase{T}"/> 编写自定义测点（如 JSON 反序列化到特定类型）
/// 2. 通过手动调用 <see cref="TagsProject_Extensions.AddSimpleFilesTagBuilder"/> 注册基本类型测点构建器，
///    再配合自定义的 TagBuilder，实现「基本类型读写 + JSON 到特定类型读写」共存
/// </summary>
public class SimpleFilesDirectTagExtensionTests
{
    #region 外部扩展的自定义类型（模拟用户自己的扩展代码）

    /// <summary>
    /// 测试用的 JSON POCO
    /// </summary>
    record JsonPoint(int X, int Y);

    /// <summary>
    /// 外部自定义的 JSON 测点：继承 <see cref="SimpleFilesDirectTagBase{T}"/>，<br/>
    /// 文件内容为 JSON 文本，反序列化为 <see cref="JsonPoint"/> 类型的测点值。<br/>
    /// 只需实现对称的 <see cref="SimpleFilesDirectTagBase{T}.ParseValue"/> 与 <see cref="SimpleFilesDirectTagBase{T}.FormatValue"/>，
    /// 读写与自动创建默认文件均由基类完成。
    /// </summary>
    class JsonPointDirectTag : SimpleFilesDirectTagBase<JsonPoint>
    {
        /// <summary>
        /// c'tor
        /// </summary>
        public JsonPointDirectTag(TagDescriptor descriptor, SimpleFilesTagChannel? thisChannel, TagContainer container)
            : base(descriptor, thisChannel, container)
        {
        }

        /// <inheritdoc/>
        protected override JsonPoint ParseValue(string text)
        {
            var value = JsonSerializer.Deserialize<JsonPoint>(text);
            return value ?? throw new InvalidDataException($"无法将文件内容反序列化为 {nameof(JsonPoint)}：{text}");
        }

        /// <inheritdoc/>
        protected override string FormatValue(JsonPoint? value)
        {
            var val = value ?? new JsonPoint(0, 0);
            return JsonSerializer.Serialize(val);
        }
    }

    /// <summary>
    /// 外部自定义的 TagBuilder：为 SimpleFiles 驱动下 TagKind=="JSON" 的测点构建 <see cref="JsonPointDirectTag"/>
    /// </summary>
    public class JsonPointTagBuilder : TagBuilderBase
    {
        /// <summary>
        /// c'tor
        /// </summary>
        public JsonPointTagBuilder()
        {
        }

        /// <inheritdoc/>
        public override ITag Build(ITagChannel channel)
        {
            if (channel is not SimpleFilesTagChannel simpleFilesChannel)
            {
                throw new InvalidCastException($"测点({this.Name})当前通道必须是{nameof(SimpleFilesTagChannel)}，实际={channel.GetType()}");
            }
            
            this.TagDescriptor.NormalizedAddress = simpleFilesChannel.MakePath(this.TagDescriptor.RawAddress);
            var thisChannel = this.Channel as SimpleFilesTagChannel;
            return new JsonPointDirectTag(this.TagDescriptor, thisChannel, TagContainer.From(this.Parent));
        }
    }

    #endregion

    #region Helper

    /// <summary>
    /// 创建独立临时目录，测试结束由调用方清理
    /// </summary>
    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "SimpleFilesExtension_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// 构建测试 XML：包含一个基本类型测点(int-v)和一个 JSON 测点(json-v)<br/>
    /// json-v 配置了 AutoCreateFile=true，验证基类基于 <see cref="SimpleFilesDirectTagBase{T}.FormatValue"/> 自动创建默认文件的能力
    /// </summary>
    private static XElement BuildTestXml(string baseDir)
    {
        return XElement.Parse($@"
<root>
    <Channel name='sf' driver='SimpleFiles'>
        <BaseDir>{baseDir}</BaseDir>
    </Channel>
    <TagGrp name='g' isEntry='true' channel='sf' scanInterval='0'>
        <Tag name='int-v'   address='int.txt'    type='INT32' />
        <Tag name='json-v'  address='point.json' type='JSON' AutoCreateFile='true' />
    </TagGrp>
</root>");
    }

    /// <summary>
    /// 注册：通道 + 自定义 JSON builder + 基本类型 builder（通过 AddSimpleFilesTagBuilder）
    /// </summary>
    private static ServiceProvider BuildRoot()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTagsProjectServices(b =>
        {
            // 1. 仅注册通道
            b.AddSimpleFilesChannel();

            // 2. 注册外部自定义的 JSON 测点构建器（先注册，predicate 命中 JSON 测点）
            b.ConfigTagsLoader((sp, composite) =>
                composite.AddTagBuilder<JsonPointTagBuilder>(
                    SimpleFilesNames.DriverName,
                    predicate: bd => bd.TagDescriptor.TagKind == "JSON"));

            // 3. 注册基本类型测点构建器（后注册，处理其余所有测点）
            b.AddSimpleFilesTagBuilder();
        });
        return services.BuildServiceProvider();
    }

    #endregion

    #region 验证：基本类型 + JSON 测点共存并能加载

    [Fact]
    public void AddSimpleFilesTagBuilder_WithCustomJsonBuilder_LoadsBothBasicAndJsonTags()
    {
        var tempDir = CreateTempDir();
        try
        {
            using var root = BuildRoot();
            using var scope = root.CreateScope();
            var sp = scope.ServiceProvider;

            using var proj = sp.MakeProject(null, BuildTestXml(tempDir));

            // 通道
            Assert.Single(proj.Channels);
            Assert.IsType<SimpleFilesTagChannel>(proj.Channels[0]);

            // 两个测点都加载成功
            var g = proj.Tags.SelectGrp("g");
            Assert.NotNull(g);

            var intTag = g.SelectTag("int-v");
            Assert.NotNull(intTag);
            Assert.Equal(BuiltinTagKinds.INT32, intTag.TagKind());

            var jsonTag = g.SelectTag("json-v");
            Assert.NotNull(jsonTag);
            Assert.Equal("JSON", jsonTag.TagKind());
            // 外部自定义的 JSON 测点类型被正确构建
            Assert.IsType<JsonPointDirectTag>(jsonTag);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    #endregion

    #region 验证：基本类型读写（走 AddSimpleFilesTagBuilder 注册的内部构建器）

    [Fact]
    public async Task BasicIntTag_WriteThenRead_RoundTrips()
    {
        var tempDir = CreateTempDir();
        try
        {
            using var root = BuildRoot();
            using var scope = root.CreateScope();
            var sp = scope.ServiceProvider;

            using var proj = sp.MakeProject(null, BuildTestXml(tempDir));
            var g = proj.Tags.SelectGrp("g");
            var intTag = g.SelectTag("int-v");

            // 预创建文件（模拟文件已存在的场景；AutoCreateFile=false 时文件不存在会直接跳过写入）
            var intPath = Path.Combine(tempDir, "int.txt");
            await File.WriteAllTextAsync(intPath, "0");

            // 写入
            intTag.Value = 42;
            await intTag.WriteAsync(CancellationToken.None);
            Assert.False(intTag.IsDirty);
            Assert.Equal("42", await File.ReadAllTextAsync(intPath));

            // 读取
            intTag.Value = 0;
            await intTag.ReadAsync(CancellationToken.None);
            Assert.Equal(42, intTag.Value);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    #endregion

    #region 验证：JSON 测点读写（外部自定义 builder + 继承 SimpleFilesDirectTagBase<T>）

    [Fact]
    public async Task JsonTag_WriteAsync_SerializesJsonToFile()
    {
        var tempDir = CreateTempDir();
        try
        {
            using var root = BuildRoot();
            using var scope = root.CreateScope();
            var sp = scope.ServiceProvider;

            using var proj = sp.MakeProject(null, BuildTestXml(tempDir));
            var g = proj.Tags.SelectGrp("g");
            var jsonTag = g.SelectTag("json-v");

            // 写入：POCO → JSON 文件
            jsonTag.Value = new JsonPoint(3, 4);
            await jsonTag.WriteAsync(CancellationToken.None);
            Assert.False(jsonTag.IsDirty);

            var content = await File.ReadAllTextAsync(Path.Combine(tempDir, "point.json"));
            Assert.Equal("{\"X\":3,\"Y\":4}", content);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task JsonTag_ReadAsync_DeserializesJsonFromFile()
    {
        var tempDir = CreateTempDir();
        try
        {
            // 预置 JSON 文件内容
            await File.WriteAllTextAsync(Path.Combine(tempDir, "point.json"), "{\"X\":7,\"Y\":9}");

            using var root = BuildRoot();
            using var scope = root.CreateScope();
            var sp = scope.ServiceProvider;

            using var proj = sp.MakeProject(null, BuildTestXml(tempDir));
            var g = proj.Tags.SelectGrp("g");
            var jsonTag = g.SelectTag("json-v");

            // 读取：JSON 文件 → POCO
            await jsonTag.ReadAsync(CancellationToken.None);
            Assert.Equal(new JsonPoint(7, 9), jsonTag.Value);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task JsonTag_AutoCreateFile_WhenFileMissing_CreatesDefaultJson()
    {
        var tempDir = CreateTempDir();
        try
        {
            // 文件不存在（json-v 已配置 AutoCreateFile=true）
            var jsonPath = Path.Combine(tempDir, "point.json");
            Assert.False(File.Exists(jsonPath));

            using var root = BuildRoot();
            using var scope = root.CreateScope();
            var sp = scope.ServiceProvider;

            using var proj = sp.MakeProject(null, BuildTestXml(tempDir));
            var g = proj.Tags.SelectGrp("g");
            var jsonTag = g.SelectTag("json-v");

            // ReadAsync：文件缺失 → 基类调用 CreateAndWriteDefaultAsync → FormatValue(default) 生成默认 JSON
            await jsonTag.ReadAsync(CancellationToken.None);

            // 默认文件被创建，内容为 FormatValue(null) 产出的合法默认 JSON
            Assert.True(File.Exists(jsonPath));
            var content = await File.ReadAllTextAsync(jsonPath);
            Assert.Equal("{\"X\":0,\"Y\":0}", content);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    #endregion
}
