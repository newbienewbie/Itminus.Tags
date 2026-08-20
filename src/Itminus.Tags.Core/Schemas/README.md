# TagsProject XML Schema（XSD）

## 架构：schema 随驱动项目归属

XSD 源文件**按驱动归属**存放在各项目自己的 `Schemas/` 目录，而非集中存放：

| 文件 | 所在项目 |
|---|---|
| `Schemas/tagsproject.xsd`（核心：Project/TagGrp/TagCbnt/Tag + Channel） | `Itminus.Tags.Core` |
| `Schemas/s7.xsd` | `Itminus.Tags.S7` |
| `Schemas/modbustcp.xsd` | `Itminus.Tags.ModbusTcp` |
| `Schemas/zlan.xsd` | `Itminus.Tags.ZLan` |
| `Schemas/hjzk.xsd` | `Itminus.Tags.Hjzk` |
| `Schemas/com.xsd` | `Itminus.Tags.ComScanner` |
| `Schemas/opcua-client.xsd` | `Itminus.Tags.OpcUaClient` |
| `Schemas/simplefiles.xsd` | `Itminus.Tags.SimpleFiles` |

每个内置项目把自己的 XSD 交给源生成器（`src/Itminus.Tags.SchemaGenerator`）编译为静态字符串常量
（类 `SchemaContent_{文件名}`，命名空间 `Itminus.Tags.Generated`）——**不依赖资源反射，对 NativeAOT/trim 友好**。
这是内置驱动的内部实现选择；**第三方驱动不要求使用源生成器**（见下文"第三方驱动库"）。

## 三种存在形式

| 形式 | 用途 |
|---|---|
| 各项目 `Schemas/`（源） | 本仓库开发时编辑器校验、维护 |
| **DLL 内（源生成器编译为常量）** | **运行期可选校验**（`EnableXmlSchemaValidation()`），随包自动部署 |
| **NuGet `buildTransitive` 注入** | **应用方编辑器智能提示**：还原包后自动以链接项出现 |

## 应用方：编辑器智能提示（零配置）

还原 `Itminus.Tags.Core` + 用到的驱动包后，项目树自动出现（由各包 `buildTransitive` targets 注入）：

```
<项目根>/Schemas/
├── tagsproject.xsd        # Core 注入（核心 schema）
└── Drivers/               # 各驱动包注入
    ├── s7.xsd  
    ├── com.xsd  
    ├── modbustcp.xsd 
    ├── ...
```

`index.xml` 根元素上写：

```xml
<Project xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      xsi:noNamespaceSchemaLocation="Schemas/tagsproject.xsd"
      xmlns:s7="tags:s7">
  <Channel name="S7-1" driver="S7">
    <s7:IpAddr>localhost</s7:IpAddr>
    ...
  </Channel>
</Project>
```
Core 不承载驱动映射，驱动专属子元素（`<s7:IpAddr>` 等）的编辑器校验/补全，需把驱动命名空间映射到驱动 schema
（vscode-xml 用 `xml.schemas` 设置，Oxygen/XMLSpy 用 XML Catalog 指向各驱动 `Schemas/*.xsd`）。
仓库内示例见 `.vscode/settings.json` 的 `xml.schemas`。


关闭自动注入：`<TagsProjectSchemaDisabled>true</TagsProjectSchemaDisabled>`。

## 运行期校验（可选功能）

```csharp
services.AddTagsProjectServices(b =>
{
    b.EnableXmlSchemaValidation(); // 启用加载期校验以提前抛出 TagsProjectSchemaException
    b.AddS7Support();              // 各 AddXxxSupport 会自动注册各自的 schema provider
    ...
});
```

## 第三方驱动库：嵌入自己的 schema 校验

第三方驱动**只需要做两件事**，不要求依赖 `Itminus.Tags.SchemaGenerator`：

### 1. 运行时校验：实现 `ITagsProjectSchemaProvider`（DI 注册）

契约只有一条：该方法返回 `IEnumerable<(string LogicalName, string Content)>`（逻辑名 → XSD 文本）。
内容来源完全自由——EmbeddedResource、磁盘文件、硬编码字符串、源生成器均可：

```csharp
// 方式一：EmbeddedResource（xsd 作为嵌入资源）
public sealed class MyDriverSchemaProvider : ITagsProjectSchemaProvider
{
    public IEnumerable<(string LogicalName, string Content)> GetSchemaContents()
    {
        var asm = typeof(MyDriverSchemaProvider).Assembly;
        var names = asm.GetManifestResourceNames().Where(n => n.EndsWith(".xsd"));
        foreach (var n in names)
        {
            using var s = asm.GetManifestResourceStream(n)!;
            using var r = new StreamReader(s);
            yield return (n, r.ReadToEnd());
        }
    }
}
```

```csharp
// 方式二：源生成器（可选，与内置驱动一致，AOT/trim 友好）
//   引用 Itminus.Tags.SchemaGenerator，AdditionalFiles 传入自己的 xsd，
//   生成器自动生成 SchemaContent_{文件名} 类：
public sealed class MyDriverSchemaProvider : ITagsProjectSchemaProvider
{
    public IEnumerable<(string LogicalName, string Content)> GetSchemaContents() =>
        SchemaContent_mydriver.GetSchemaContents();
}
```

注册：

```csharp
services.AddTagsProjectServices(b =>
{
    b.EnableXmlSchemaValidation();
    b.Services.AddSingleton<ITagsProjectSchemaProvider, MyDriverSchemaProvider>();
    ...
});
```

启用校验后，校验器把核心 + 所有 provider 的 schema 合并进 `XmlSchemaSet`。

### 2. 编辑器智能提示：buildTransitive targets 注入

自己的 `buildTransitive` targets 把 xsd 以链接项注入消费项目（与内置驱动一致）：

```xml
<Project>
  <ItemGroup Condition="'$(TagsProjectSchemaDisabled)' != 'true'">
    <None Include="$(MSBuildThisFileDirectory)..\Schemas\mydriver.xsd"
          Link="Schemas\Drivers\mydriver.xsd" />
  </ItemGroup>
</Project>
```

> 注：运行时校验（步骤 1）与编辑器注入（步骤 2）相互独立。没有硬性要求全部完成。
