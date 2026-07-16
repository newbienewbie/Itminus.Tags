

一个免费开源的工业通讯测点库。

## 文档

[制作中]

## 授权方式

本仓库由许多子包构成，根据上游依赖的不同，我们为每个子包采用不同的授权协议。基本原则是**在尊重上游依赖包授权的前提下，选择最友好的开源授权协议** (基本都是 **MIT**)。

### 术语解释

- **官方类库**：这里所称**官方类库** 是指 **.NET BCL** 或者 **[dotnet](https://github.com/dotnet/runtime) 的官方扩展包**(MIT协议)


### 核心包和无第三方依赖的实现包 

核心类库部分，由于不涉及官方类库之外的第三方依赖，均采用**MIT协议**: 
- Itminus.Tags.Core
- Itminus.Tags

部分硬件实现包，由于不涉及官方类库之外的第三方依赖，也采用**MIT协议**:
- Itminus.Tags.ComScanner

部分包装（Wrapper）实现包，上游依赖均为 MIT 协议，同样采用**MIT协议**：
- Itminus.Tags.BlazorLib

### 涉及第三方依赖的实现包：

上游是[MIT](https://github.com/NModbus/NModbus)授权，所以这里我们也一律采用MIT授权:

- Itminus.Tags.ModbusTcp
- Itminus.Tags.Hjzk
- Itminus.Tags.ZLan
- Itminus.Tags.S7
- Itminus.Tags.RxExtensions

**OPC UA** 相关，我记得**早期**OPC基金会的仓库下全是GPL授权，不过最近我发现它们官方已经**改成了[OPC Foundation MIT License 1.00](https://github.com/OPCFoundation/UA-.NETStandard/blob/master/LICENSE.txt)**，所以我们也遵循这个开源协议——**OPC Foundation MIT License 1.00**：
- Itminus.Tags.OpcUaClient
