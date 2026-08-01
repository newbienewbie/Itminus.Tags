
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
- [x] ModbusTcp：读按 PDU 上限分批并补测试——读保持/输入寄存器(FC03/FC04)每帧默认 125 个、读线圈/离散输入(FC01/FC02)每帧默认 2000 点，超出自动拆帧；单帧上限可通过 `MaxReadRegisters`/`MaxReadBits` 配置（某些设备上限小于协议值），配置值超协议上限时加载期报错。
- [x] ModbusTcp：写路径按 123 寄存器分批；`MaxBatchSize` 重命名为 `MaxWriteRegisters`，与 `MaxReadRegisters`/`MaxReadBits` 对齐成 `Max{Action}{Unit}` 命名族（XML 元素名同步变更，1.0 前破坏性变更）。
- [x] ModbusTcp：清理 `WriteAsync` 中 `currlen < 0` 的恒假死检查——`while(true)+break` 改为 `while(offset < payload.Length)`，循环条件即不变量，死检查与 `currlen==0` 分支一并移除。
- [x] ModbusTcp：测试覆盖率从 58% 提升到 79%（包整体 line-rate）——补足 DirectTag 家族：MultipleBytes 家族（Float/Int32/Int64/UInt16/UInt32/UInt64）读写与字节序、ByteDirectTag 的读写/字节序/异常路径、4 个 BitLikes（HoldingRegisterBit/InputRegisterBit/OutputCoil/InputContact）的读写/置位/清位/只读写入抛错。
- [x] ModbusTcp DirectTag 重构（方案B）：BitLikes 与 ByteDirectTag 不再直接访问 `ModbusMaster`，改为走通道层 `ReadAsync`/`WriteAsync`——修复 NthBit 8~15 的位寻址 bug（原代码把位索引当寄存器索引，读到相邻寄存器）；`ModbusMaster` 改 `internal` 不再暴露公共 API。
- [x] ModbusTcp DirectTag 测试重构：BitLikes 与 ByteDirectTag 测试改用 `TestModbusTcpChannel` + Moq `IModbusMaster`，走真实通道层（地址解析、偶数校验、分批、`UShortsToBytes` 小端转换全被真实执行），弃用重写通道层的 fake——顺带修复被 fake 掩盖的 3 个 bug：`GetBufferSize()` 读 1 字节触发通道层偶数校验异常（改为恒 2 字节，寄存器 16 位含 bit0~15）、`ByteDirectTag` 高低字节取反、`ByteDirectTag` 写回字节序错误。补 NthBit 8~15 回归测试。
- [ ] ModbusTcp DirectTag 遗留：MultipleBytes 家族（Float/Int32/Int64/UInt16/UInt32/UInt64）测试仍用 fake 通道，其字节序在真实通道层下的正确性存疑（16 位类型的 `EndianKind` 处理可能矛盾）——需改用 Moq 并审视。
- [x] ModbusTcp 字节序语义修正（DirectTag 路径）：确立"通道层 cache 固定每寄存器低字节在前，EndianKind 描述设备寄存器内部存储序"模型——16 位类型交换分支（设备大端→cache 小端读，设备小端→cache 大端读）；32/64/Float 的 BigEndian 分支改为逐寄存器(2字节)交换后大端读/写（基类新增 `SwapEachRegister`）；测试全部改 Moq 走真实通道层，补 LittleEndian 回归。**注意**：Core 的数值 Cbntor（`Int16TagCbntor` 等公共类）有相同字节序模式，但影响面含 S7（S7 的 cache 布局可能不同），需独立一轮确认后修复。
- [x] ModbusTcp 通道层字节转换自研：移除对 `MarshalHelper.UShortsToBytes`/`BytesToUShorts` 的依赖（该 helper 是 `Buffer.BlockCopy` 内存重解释，大端 CPU 会给出大端展平破坏 cache 契约）——改为自研 `ToLittleEndianBytes`/`FromLittleEndianBytes`：输入 `ReadOnlySpan<T>`、输出改为"写入调用方提供的 `Span` 目标"（分配权移交调用方，为 cache 复用铺路）；小端 CPU 走 `MemoryMarshal.AsBytes/Cast` 块拷贝（高频最优），大端 CPU 走显式小端循环（保跨端契约）；调用点用 `CollectionsMarshal.AsSpan` 消除 `List.ToArray()` 分配；`IsLittleEndianOverride` 可空实例属性注入（null 回退 `BitConverter.IsLittleEndian`，避免静态字段污染全局），测试覆盖大端模拟路径。写路径增加快路径：≤ `MaxWriteRegisters` 寄存器时一次整体写入，避免分批与切片分配；空 payload 直接返回。**遗留**：`FutureTech.Protocols`/`FutureTech.ModbusTcp.Options` 已无直接引用，可独立轮次评估移除。

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
- [ ] 待查：Modbus优化时发现一个VS和CLI构建行为不一致的问题，见 backlog.md。
- [ ] 性能：Modbus 读路径 cache 复用——`TagCbnt.ReadAsync` 每轮轮询 `this.Cache = bytes.AsMemory()` 换新引用，`CacheSize` 不变时可复用同一 `byte[]` 消除每轮分配；需接口改动（`IContinuousBytesBasedTagChannel.ReadAsync` 改为写入预分配 buffer 返回字节数），影响所有驱动，独立轮次设计。
- [ ] 性能：Modbus 写路径消除 `FromLittleEndianBytes` 的物化——当前 NModbus `WriteMultipleRegistersAsync` 要求 `ushort[]`，且 `Span<ushort>` 不能跨 async await（ref struct 限制），故保留 dest 物化形态（1 分配 1 拷贝）。将来 NModbus 若支持 Span/Memory 参数，可在 await 调用内传 `MemoryMarshal.Cast<byte,ushort>(bytes)` 视图（小端 CPU）直接消除拷贝，helper 的 dest 形态不阻塞此演进。



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
