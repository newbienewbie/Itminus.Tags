

一个免费开源的跨平台工业通讯测点库。**你只管提供描述(`xml`)，我们负责让它跑起来**。

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

await ctrl.StartPollAsync(dir, root, hook: async(proj, ct) =>{
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

- 支持逻辑组件插件
- 支持通过MCP方式暴露给AI来读写测点。

## 文档

[制作中，预览版可以查看：http://tags.doc.stdunit.com ]

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
