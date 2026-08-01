
这是一个面向工业通讯场景的类库：

* 免费开源: 整个类库家族都是MIT授权，而且相关依赖链也都是(或近乎是)MIT授权。
* 高度模块化: 每种硬件实现，以`nuget`包为单元，各自独立。
* 易于扩展：照抄内置的设备实现，实现你自己的通讯封装，然后编写一个`.AddYourOwnSupport()`扩展方法。
* 跨平台：依托于`dotnet`跨平台的能力，让你的代码跑到各种设备上。

> **在正式发布1.0版本之前，这个包只会发布在我的测试源上**。
> 如果你使用`nuget`管理，请参照[示例](https://github.com/newbienewbie/Itminus.Tags.WPFDemo/blob/867a5063bc65ec16f77692d4c56ce9da5a38dc3c/nuget.config#L3-L8)，指定包源为 https://baget.stdunit.com/v3/index.json ；
> 如果你使用`paket`管理，参照本项目[paket.dependencies](https://github.com/newbienewbie/Itminus.Tags/blob/b4ef40f2952fa75d7154db03782c2b5f98be914c/paket.dependencies#L1-L2) 指定包源。
> 我个人建议你使用`paket`管理依赖，这样哪怕我和nuget.org都破产跑路了，你的本地代码也能完全断网的情况下离线编译。

警告：假设版本号是`<major>.<minor>.<patch>`:
- 在`v1.0`版本之前，每个`minor`版本的跳变，可能会引入新特性和破坏性更新。
- 在`v1.0`版本之后，每个`major`版本的跳变，可能会引入新特性和破坏性更新。

### Todo

**0.11.0 之前的 todo:**
- [x] 拼写错误修正：`BaundRate` -> `BaudRate`、`IContinous` -> `IContinuous`（含公共 API、示例与测试中的 XML）。
- [x] 公开接口不应该暴露 `FSharpResult`：`ModBusTcpAddressParser.ParseWithNthBit` / `ParseWithoutNthBit` 改为 `internal`（S7 已如此，Modbus 只差两个访问修饰符）。
- [ ] ModbusTcp：读写按 PDU 上限（125 寄存器）分批，补测试；顺带清理 `WriteAsync` 中 `currlen < 0` 的恒假死检查。

**1.0 之前的 todo:**
- [ ] ModbusTcpChannel 和 OpcUaClientChannel 多入口的串行化。目前这两种驱动还不支持并行，意味着同一个通道不能给多个入口使用。落地方案参考 ComScanner 的分锁设计（连接/读/写分别加锁），避免一把大锁造成队头阻塞；串行化必须覆盖 `EnsureConnectedAsync`，消除并发创建连接/会话的竞态。
- [ ] OpcUaClientChannel 的具体问题清理：删除从未使用的 `_connSignal` 字段；`WriteAsync` 不再丢弃调用方的 `ct`；检查 `ReadValuesAsync` 返回的错误（坏值不进缓存）；去掉 `Bag` 的双重写入；证书默认值（`AutoAcceptUntrustedCertificates` 等）改为可配置并输出警告。
- [ ] 为项目描述 XML 引入 schema 校验机制：XSD/DTD，外加加载期的交叉引用校验（如 `channel` 属性必须能在已声明的 `<Channel>` 中找到，拼错的通道名在加载期报错而不是运行时才暴露）。
- [ ] 异常细化：为特定场景编写特定异常类型，目前大多是裸 `Exception`/`ArgumentException`/`InvalidOperationException` 等；重名测点报错带上完整路径上下文。
- [ ] TagsProjectCtrl 的清理路径的空 `catch` 被有意设计成了静默吞掉异常（如 `StartPollAsync`/`StopAsync` 中的 `Dispose`/`DisconnectAsync`），但应该补上日志，增加可观测性、不改变吞掉异常的语义。

**1.0 之后的 todo (backlog):**
- [ ] S7 优化：底层基于 Sharp7 一个古老的实现，有两个优化的点：
    - 底层通道基于 Sharp7 一个古老的同步实现，用了 `Thread` 伪装成异步接口。将来可以改成真异步（上层异步接口不需要变更）。
    - 底层有很多无谓的字节拷贝和内存分配操作，借助 C# 的 `Span<T>` 和 `Memory<T>` 可以大幅优化实现。
- [ ] 文档完善和更新：现在文档库和主项目库分离，因为代码在快速更新，没时间同步完善文档。`docs/`打算只放一些基本的框架性的东西，详细的使用说明放到独立的文档库。时间戳/告警/心跳语义文档化：谁在什么时候写入 `Timestamp`、断线重连与数据有效性（stale）的系统行为。
- [ ] 日志、报错提示、注释文档的多语言支持。
- [ ] 工程化：启用 NetAnalyzers / `TreatWarningsAsErrors`；CI 加入测试覆盖率门槛。

## Quick Start

**你只管提供描述(`xml`)，我们负责让它跑起来**。

其中，你提供的描述类似于：
```xml
<root>
	<!-- 通道，可以配置多个-->
	<Channel name="S7-1" driver="S7" >
		<IpAddr>172.16.10.20</IpAddr>
		<Rack>0</Rack>
		<Slot>1</Slot>
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
</root>
```

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

    // ...可选：如注册 proj.TurnStarted 或者 projCrashed 事件处理
    return Task.CompletedTask;
});
```

优势：
- 硬件无关抽象：理论上，你可以在家里用[S7模拟器](https://github.com/newbienewbie/S7SvrSim)编写自动化测试，验证你的逻辑，最后到现场前再切换到`OpcUa`设备上(或者反过来)。
- 支持逻辑组件插件(dll)
- 支持通过MCP方式暴露给AI：把测点项目描述作为上下文，AI可以轻松操作点位

![]()

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
	- `Itminus.Tags.Core`: 核心抽象
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

## 授权方式

本仓库由许多子包构成，根据上游依赖的不同，我们为每个子包采用不同的授权协议。基本原则是**在尊重上游依赖包授权的前提下，选择最友好的开源授权协议** (基本都是 **MIT**)。

### 术语解释

- **官方类库**：这里所称**官方类库** 是指 **.NET BCL** 或者 **[dotnet](https://github.com/dotnet/runtime) 的官方扩展包**(MIT协议)


### 核心包和无第三方依赖的实现包 

核心类库部分和部分硬件实现包，由于不涉及官方类库之外的第三方依赖，均采用**MIT协议**: 
- Itminus.Tags.Core
- Itminus.Tags
- Itminus.Tags.BlazorLib.Core
- Itminus.Tags.BlazorLib
- Itminus.Tags.ComScanner


### 涉及第三方依赖的实现包：

目前本项目下的依赖，除了**OPC UA**之外，所有的上游依赖都是[MIT](https://github.com/NModbus/NModbus)授权，所以这里我们也一律采用MIT授权:

- Itminus.Tags.RxExtensions
- Itminus.Tags.R3Extensions
- Itminus.Tags.ModbusTcp
- Itminus.Tags.Hjzk
- Itminus.Tags.ZLan
- Itminus.Tags.S7

目前唯一比较特殊的是**OPC UA**，我记得**早期**OPC基金会的仓库下基本都是GPL授权，不过最近我发现它们官方已经**改成了[OPC Foundation MIT License 1.00](https://github.com/OPCFoundation/UA-.NETStandard/blob/master/LICENSE.txt)**，所以我们也遵循这个开源协议——**OPC Foundation MIT License 1.00**：
- Itminus.Tags.OpcUaClient
