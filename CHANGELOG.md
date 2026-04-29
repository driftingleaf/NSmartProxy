# Changelog

本文档记录当前仓库在本轮整理中的主要变更。

## 2026-04-29

### Added

- 新增中文升级说明文档：[docs/2026-04-29-net10-upgrade-and-optimization.md](</D:/Work/QuestionByAi/NSmartProxy/docs/2026-04-29-net10-upgrade-and-optimization.md>)
- 新增中文架构评审与原理说明文档：[docs/2026-04-29-nsmartproxy-principle-and-optimization-review.md](</D:/Work/QuestionByAi/NSmartProxy/docs/2026-04-29-nsmartproxy-principle-and-optimization-review.md>)
- 新增池化缓冲包装类型：[src/NSmartProxy.Infrastructure/PooledByteBuffer.cs](</D:/Work/QuestionByAi/NSmartProxy/src/NSmartProxy.Infrastructure/PooledByteBuffer.cs>)

### Changed

- 将服务端与客户端主链路升级到 `.NET 10`
- 关闭 `PublishTrimmed`，避免反射、`LiteDB`、`Newtonsoft.Json`、`log4net` 等场景带来的裁剪运行时风险
- 将 `log4net` 升级到 `3.3.1`
- 优化主隧道读写链路，降低临时对象和大块内存分配
- 将热路径中的小包处理改为 `Span<T>`、`Memory<T>`、`BinaryPrimitives` 风格
- 优化 `Snappy` 压缩与解压路径，减少中间数组分配
- 改善长连接与流式传输场景下的半关闭处理
- 为关键 TCP 连接启用 `NoDelay`
- 补充 `.gitignore`，忽略 `.dotnet-home/`、`.tmpobj/`、`*.lscache` 等本地缓存

### Performance

- 将历史实现中极大的传输缓冲调整为更合理的固定大小，并接入缓冲池复用
- 优化客户端与服务端双向转发循环，减少 GC 压力
- 优化控制帧和长度头编码逻辑，降低高频小分配
- 优化压缩模式下的数据包收发路径，减少压缩前后复制次数

### Fixed

- 修复若干超时处理中的旧式等待逻辑
- 修复部分读写实现中的短读与边界处理问题
- 修复部分过时加密与证书加载 API 在新框架下的兼容性问题
- 修复若干连接关闭与关闭通知流程中的稳定性问题

### Docs

- 补充 `NSmartProxy` 内网穿透实现原理、链路说明、架构图和后续优化建议
- 补充本轮升级与发布产物说明，便于后续交接和部署
