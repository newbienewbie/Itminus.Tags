
# Backlog / 开发计划


## v1.0 之前的 todo

v1.0 之前只专注于正确性和可靠性，我不推荐外部人员使用——每多在一个现场用，就可能为我将来引入的不兼容改动增加一些负担。

- [x] 轮询清理路径修复：`TagGrpRunner` 清理路径不再用已取消的 ct 调 `DisconnectAsync`（原实现 S7 会因 `_rw.WaitAsync(ct)` 立即抛 OCE 导致连接不断开、资源泄漏）。
- [x] 通道断开策略化：新增 `ITagGrpRunnerDisconnectStrategy`（Core 公共接口）+ `DefaultTagGrpRunnerDisconnectStrategy`（有限超时等待，默认 5s），可按设备注入不同断开等待策略；`TagGrpRunnerFactory` 从 DI 解析。
- [ ] OpcUaClientChannel 的具体问题清理：`WriteAsync` 不再丢弃调用方的 `ct`；检查 `ReadValuesAsync` 返回的错误（坏值不进缓存）；去掉 `Bag` 的双重写入；证书默认值（`AutoAcceptUntrustedCertificates` 等）改为可配置并输出警告。
- [ ] TagsProjectCtrl 的清理路径的空 `catch` 被有意设计成了静默吞掉异常（如 `StartPollAsync`/`StopAsync` 中的 `Dispose`/`DisconnectAsync`），但应该补上日志，增加可观测性、不改变吞掉异常的语义。
- [ ] 异常细化：为特定场景编写特定异常类型，目前大多是裸 `Exception`/`ArgumentException`/`InvalidOperationException` 等；重名测点报错带上完整路径上下文。优先做**加载期错误**（XML/地址/配置）统一异常族——便宜且对库用户价值高。
- [ ] OpcUa和ModbusTcp通道串行化（多入口并发）。说明：当前S7已经做了单通道多入口的串行化，OpcUa和ModbusTcp目前只支持"单通道单入口"模型。这是一个值得改进的方向，可以参考 ComScanner/S7 设计横展。
- [ ] 为项目描述 XML 引入 schema 校验机制：XSD/DTD，外加加载期的交叉引用校验（如 `channel` 属性必须能在已声明的 `<Channel>` 中找到，拼错的通道名在加载期报错而不是运行时才暴露）。
- [ ] 文档完善和更新：`docs/` 放框架性内容，详细使用说明放独立文档库。

## v1.0 之后的 todo

嗯，v1.0 

- [ ] 日志、报错提示、注释文档的多语言支持。
- [ ] 工程化：
  - [x] CI 强制测试（`.github/workflows/dotnet.yml` 构建+测试、`release.yml`）
  - [ ] `coverlet.collector` 已引入但未做覆盖率门槛；
  - [ ] 启用 NetAnalyzers / `TreatWarningsAsErrors`；
- [ ] Cache性能优化：Modbus 读路径 cache 复用——`TagCbnt<T>` 各驱动的 `ReadAsync` 每轮轮询换新 `Cache` 引用（如 `ModbusRegisterTagCbnt` 的 `this.Cache = regs`、位空间 `this.Cache = bits`），`CacheSize` 不变时可复用同一 `T[]` 消除每轮分配；需接口改动（`IModbusRegisterChannel.ReadRegistersAsync`/`IModbusBitsChannel.ReadBitsAsync` 改为写入预分配 buffer 返回元素数）
- [ ] S7 优化：底层基于 Sharp7 一个古老的实现，有两个优化的点：
    - 底层通道基于 Sharp7 一个古老的同步实现，用了 `Thread` 伪装成异步接口。将来可以改成真异步（上层异步接口不需要变更）。说明：PLC的并发吞吐能力远小于上位机，而且目前已经在单通道做了串行轮询机制，所以这个改进的收益不大。
    - 底层有很多无谓的字节拷贝和内存分配操作，借助 C# 的 `Span<T>` 和 `Memory<T>` 可以大幅优化实现。