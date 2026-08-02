
# Backlog / 开发计划

> 从 `README.md` 迁出的开发待办。README 只保留入口指引，详见各节。

## 1.0 之前的 todo

- [ ] ModbusTcpChannel 和 OpcUaClientChannel 多入口的串行化。目前这两种驱动还不支持并行，意味着同一个通道不能给多个入口使用。落地方案参考 ComScanner 的分锁设计（连接/读/写分别加锁），避免一把大锁造成队头阻塞；串行化必须覆盖 `EnsureConnectedAsync`，消除并发创建连接/会话的竞态。
- [ ] OpcUaClientChannel 的具体问题清理：`WriteAsync` 不再丢弃调用方的 `ct`；检查 `ReadValuesAsync` 返回的错误（坏值不进缓存）；去掉 `Bag` 的双重写入；证书默认值（`AutoAcceptUntrustedCertificates` 等）改为可配置并输出警告。
  - 注：`_connSignal` 未使用字段已在 0.11 清理中删除。
- [ ] 为项目描述 XML 引入 schema 校验机制：XSD/DTD，外加加载期的交叉引用校验（如 `channel` 属性必须能在已声明的 `<Channel>` 中找到，拼错的通道名在加载期报错而不是运行时才暴露）。
- [ ] 异常细化：为特定场景编写特定异常类型，目前大多是裸 `Exception`/`ArgumentException`/`InvalidOperationException` 等；重名测点报错带上完整路径上下文。
- [ ] TagsProjectCtrl 的清理路径的空 `catch` 被有意设计成了静默吞掉异常（如 `StartPollAsync`/`StopAsync` 中的 `Dispose`/`DisconnectAsync`），但应该补上日志，增加可观测性、不改变吞掉异常的语义。

## 1.0 之后的 todo

- [ ] S7 优化：底层基于 Sharp7 一个古老的实现，有两个优化的点：
    - 底层通道基于 Sharp7 一个古老的同步实现，用了 `Thread` 伪装成异步接口。将来可以改成真异步（上层异步接口不需要变更）。
    - 底层有很多无谓的字节拷贝和内存分配操作，借助 C# 的 `Span<T>` 和 `Memory<T>` 可以大幅优化实现。
- [ ] 文档完善和更新：现在文档库和主项目库分离，因为代码在快速更新，没时间同步完善文档。`docs/`打算只放一些基本的框架性的东西，详细的使用说明放到独立的文档库。时间戳/告警/心跳语义文档化：谁在什么时候写入 `Timestamp`、断线重连与数据有效性（stale）的系统行为。
- [ ] 日志、报错提示、注释文档的多语言支持。
- [ ] 工程化：启用 NetAnalyzers / `TreatWarningsAsErrors`；CI 加入测试覆盖率门槛。
  - [x] CI 强制测试（`.github/workflows/dotnet.yml` 构建+测试、`release.yml`）
  - [ ] `coverlet.collector` 已引入但未做覆盖率门槛；
  - [ ] NetAnalyzers / `TreatWarningsAsErrors` 未启用。
- [ ] 性能：Modbus 读路径 cache 复用——`TagCbnt<T>` 各驱动的 `ReadAsync` 每轮轮询换新 `Cache` 引用（如 `ModbusRegisterTagCbnt` 的 `this.Cache = regs`、位空间 `this.Cache = bits`），`CacheSize` 不变时可复用同一 `T[]` 消除每轮分配；需接口改动（`IModbusRegisterChannel.ReadRegistersAsync`/`IModbusBitsChannel.ReadBitsAsync` 改为写入预分配 buffer 返回元素数），影响 S7/Modbus 两族驱动，独立轮次设计。