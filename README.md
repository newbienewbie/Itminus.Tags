
[![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/branch/dev/graph/badge.svg?token=Q0UW94C5SS)](https://codecov.io/github/newbienewbie/Itminus.Tags)

| 子项目| 说明 | 覆盖率 |
|------|------|------|
| Itminus.Tags.Core | 硬件无关的核心抽象，无外部依赖 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags_core )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags_core ) | 
| Itminus.Tags | 依赖于 Itminus.Tags.Core，补充项目、日志、插件等功能 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags ) | 
| Itminus.Tags.SimpleFiles | 简单文件支持，把测点树映射为文件树 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags_simplefiles )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags_simplefiles ) | 
| Itminus.Tags.S7 | 西门子S7通信支持 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags_s7 )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags_s7 ) | 
| Itminus.Tags.ModbusTcp | ModbusTcp通信支持 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags_modbstcp )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags_modbstcp ) | 
| Itminus.Tags.OpcUaClient  | OpcUa通信支持 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags_opcuaclient )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags_opcuaclient ) | 
| Itminus.Tags.Hjzk  | Hjzk 远程IO 通信支持 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags_hjzk )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags_hjzk ) | 
| Itminus.Tags.ZLan | ZLan 远程IO 通信支持 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags_zLan )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags_zLan ) | 
| Itminus.Tags.ComScanner | 串口通信支持 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags_comscanner )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags_comscanner ) | 
| Itminus.Tags.RxExtensions  | Rx.NET 扩展 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags_rx )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags_rx ) | 
| Itminus.Tags.R3Extensions  | R3 扩展 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags_r3 )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags_r3 ) | 
| Itminus.Tags.McpServer | McpServer 扩展 | [![codecov](https://codecov.io/github/newbienewbie/Itminus.Tags/graph/badge.svg?component=itminus_tags_mcpserver )](https://codecov.io/github/newbienewbie/Itminus.Tags/components?components%5B0%5D=itminus_tags_mcpserver ) | 



这是一个面向工业交互场景的类库：

* 免费开源: 整个类库家族都是MIT授权，而且相关依赖链也都是(或近乎是)MIT授权。
* 高度模块化: 每种硬件实现，以`nuget`包为单元，各自独立。
* 易于扩展：照抄这里内置的设备实现，实现你自己的通讯封装，然后编写一个`.AddYourOwnSupport()`扩展方法挂接上去。比如，在我的树莓派上，我基于它造了一个监控GPIO、和 Linux ProcInfo、MemInfo等系统信息的网页程序。
* 跨平台：依托于`dotnet`跨平台的能力，让你的代码跑到各种设备上。

> **在正式发布1.0版本之前，这个包只会发布在我的测试源上**。
> 如果你使用`nuget`管理，请参照[示例](https://github.com/newbienewbie/Itminus.Tags.WPFDemo/blob/867a5063bc65ec16f77692d4c56ce9da5a38dc3c/nuget.config#L3-L8)，指定包源为 https://baget.stdunit.com/v3/index.json ；
> 如果你使用`paket`管理，参照本项目[paket.dependencies](https://github.com/newbienewbie/Itminus.Tags/blob/b4ef40f2952fa75d7154db03782c2b5f98be914c/paket.dependencies#L1-L2) 指定包源。
> 我个人建议你使用`paket`管理依赖，这样哪怕我和nuget.org都破产跑路了，你的本地代码也能完全断网的情况下离线编译。

警告：假设版本号是`<major>.<minor>.<patch>`:
- 在`v1.0`版本之前，每个`minor`版本的跳变，可能会引入新特性和破坏性更新。
- 在`v1.0`版本之后，每个`major`版本的跳变，可能会引入新特性和破坏性更新。


## Quick Start

你可以仅使用这个类库中的通信功能；不过我们更推荐你采用它默认的交互方式，**你只管提供描述(`xml`)，我们负责让它跑起来**。

其中，你提供的描述类似于：
```xml
<Project xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:noNamespaceSchemaLocation="Schemas/tagsproject.xsd"
         xmlns:s7="tags:s7">
	<!-- 通道，可以配置多个-->
	<Channel name="S7-1" driver="S7" >
		<s7:IpAddr>172.16.10.20</s7:IpAddr>
		<s7:Rack>0</s7:Rack>
		<s7:Slot>1</s7:Slot>
	</Channel>

	<!-- 测点配置，可以配置多个 -->
	<TagGrp name="IoBox" isEntry="true" isEnabled="true" channel="S7-1" scanInterval="0">
		<TagGrp name="通用状态">
			<TagCbnt name="PLC" address="DB200.100" access="RO">
				<Tag name="心跳请求" address="$$100.0" type="BIT" access="RO"></Tag>
				<Tag name="状态码" address="$$102.0" type="INT16" access="RW" endian="BigEndian"></Tag>
			</TagCbnt>
			<TagCbnt name="MST" address="DB201.100" access="R1W">
				<Tag name="心跳响应" address="$$100.0" type="BIT" access="R1W"></Tag>
				<Tag name="扫描周期" address="$$102.0" type="FLOAT" access="R1W"></Tag>
			</TagCbnt>
		</TagGrp>
	</TagGrp>

	<!-- 逻辑配置，可以配置0~N个 -->
	<!--<Logicet>Samples.Plugin1.dll</Logicet>-->
</Project>
```

> 说明：
> 
> - 如果你喜欢简洁一些，也可以省掉上面XML的命名空间和Schema。代价是不再有智能提示和运行前校验。
> - 文档根元素是 `<Project>`。`xsi:noNamespaceSchemaLocation` 指向还原包后自动注入的 XSD
>   （`Itminus.Tags.Core` 的 buildTransitive targets 会把 `Schemas/tagsproject.xsd` 以链接项注入项目树），
>   编辑器即可获得智能提示/校验；驱动专属子元素（`<s7:IpAddr>` 等）带驱动命名空间前缀。
> - 运行期解析不校验根元素名与命名空间——不带前缀的老格式（`<root>` + `<IpAddr>`）在不启用
>   `EnableXmlSchemaValidation()` 时照常加载。

我们的启动代码类似于：
```c#
// 项目启停控制器
var ctrl = sp.GetRequiredService<ITagsProjectCtrl>();

var dir ="D:/manufacture/pl01/";	// 提供项目运行目录，其中有通信点表和可能用到的插件
XElement? root = null;			// 空表示使用默认的`index.xml`来配置项目

await ctrl.StartPollAsync(dir, root, hook: async(proj, sp, ct) =>{
    // 添加心跳信号逻辑
    proj.Logicets.Add(new HeartBeatLogicet(
        proj.Channels,
        proj.Tags,
        loggerFactory.CreateLogger<HeartBeatLogicet>()
    ));
	// ... 添加更多业务逻辑

    // ...可选但推荐：如注册 proj.TurnStarted 或者 projCrashed 事件处理
});
```

优势：
- 硬件无关抽象：理论上，你可以在家里用[S7模拟器](https://github.com/newbienewbie/S7SvrSim)编写自动化测试，验证你的逻辑，最后到现场前再切换到`OpcUa`设备上(或者反过来)。
- 支持逻辑组件插件(dll)
- 支持通过MCP方式暴露给AI：把测点项目描述作为上下文，AI可以轻松操作点位
- “测点即文件”: 添加`Itminus.Tags.SimpleFiles`支持，可以把测点树映射为文件树，让你轻松读写和变更配置。配合`R1W`+`IsScaned`，可以尽可能减少文件系统的访问次数。

## 文档

开发者示例: 
1. 本仓库自带的[Samples](https://github.com/newbienewbie/Itminus.Tags/tree/dev/samples): 主要用于开发验证+喂狗
2. 供新手熟悉功能[WPFDemo](https://github.com/newbienewbie/Itminus.Tags.WPFDemo): 按分支演示功能。

具体文档可以查看：[tags.doc](http://tags.doc.stdunit.com) 


## 文件夹结构

- `.config`
    - `dotnet-tools.json`: 本项目用到的 dotnet tools 配置
- `global.json`: 本项目SDK配置，目前锁定版本 `8.0.102`
- `src/`: 项目代码及测试
	- `Itminus.Tags.Core`: 核心抽象；其 `Schemas/` 目录持有项目描述 XML 的 XSD（`tagsproject.xsd`）
	- `Itminus.Tags.SchemaGenerator`: 源生成器，把各项目的 XSD 编译为 DLL 内常量（AOT/trim 友好）
	- `Itminus.Tags`: 基本功能，但和具体的硬件设备无关，只依赖于`Itminus.Tags.Core`。
	- `Itminus.Tags.RxExtensions`: `dotnet/reactive`扩展，只依赖于`Itminus.Tags.Core`
	- `Itminus.Tags.R3Extensions`: `Cysharp/R3`扩展，只依赖于`Itminus.Tags.Core`
	- `Itminus.Tags.S7`: 西门子S7协议扩展，只依赖于`Itminus.Tags` + **Sharp7**
	- `Itminus.Tags.OpcUaClient`: OpcUa客户端扩展，依赖于`Itminus.Tags` + **OpcUa**
	- `Itminus.Tags.ModbusTcp`: ModbusTcp扩展，依赖于`Itminus.Tags` + **NModbus**
	- `Itminus.Tags.Hjzk`: Hjzk IO盒子扩展，依赖于`Itminus.Tags.ModbusTcp` 
	- ... 其它硬件扩展
	- `Itminus.Tags.BlazorLib.Core`: Blazor 类库，包含核心功能抽象，以及一个极简的监控页面。
	- `Itminus.Tags.BlazorLib`: 包含一些常用硬件设备的实现。
	- `Itminus.Tags.McpServer`: 这是一个把`Itminus.Tags`暴露成 [Model Context Protocol Server](https://modelcontextprotocol.io/) 的类库。
	- `Itminus.Tags.Tests`: 上述所有子项目的测试
- `samples/`: 示例代码
- `paket.dependencies`: 用 [`paket`](https://github.com/fsprojects/Paket)管理的依赖声明
- `paket.lock`: 依赖锁定文件

## LICENSING

本仓库由许多子项目构成，根据上游依赖的不同，我们为每个子项目采用不同的授权协议。基本原则是**在尊重上游依赖包授权的前提下，选择最友好的开源授权协议** (几乎都是 **MIT**，详见各仓库下的 LICENSE)。

1. 我们自己编写的核心类库部分和部分硬件实现包，由于不涉及官方类库之外的第三方依赖，一律采用**MIT协议**。
2. 除了**OPC UA**之外，所有涉及第三方依赖的实现包，其上游依赖都是[MIT](https://github.com/NModbus/NModbus)授权，所以这里我们也放心采用**MIT协议**。
3. 目前唯一比较特殊的是**OPC UA**，我记得**早期**OPC基金会的仓库下基本都是GPL授权，不过最近我发现它们官方已经**改成了[OPC Foundation MIT License 1.00](https://github.com/OPCFoundation/UA-.NETStandard/blob/master/LICENSE.txt)**，所以我们也遵循这个开源协议——**OPC Foundation MIT License 1.00**

## 开发计划

开发计划与待办事项见 [backlog.md](backlog.md)。

