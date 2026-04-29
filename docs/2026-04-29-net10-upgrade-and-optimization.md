# 2026-04-29 .NET 10 升级与性能优化说明

## 概要

本轮工作将 NSmartProxy 当前实际使用的服务端/客户端发布链路升级到 `.NET 10`，并围绕高频中转场景做了一组偏稳定性和性能导向的优化，重点覆盖 HTTP API、流式响应和高频短连接转发。

## 影响范围

- 本次升级和优化覆盖的主链路项目：
  - `src/NSmartProxy.ServerHost`
  - `src/NSmartProxyClient`
  - `src/NSmartProxy`
  - `src/NSmartProxy.ClientRouter`
  - `src/NSmartProxy.Infrastructure`
  - `src/NSmartProxy.Data`
- 共享库中仍保留 `netstandard2.0` 的兼容目标，以避免对旧链路造成不必要影响。

## 关键改动

- 主服务端/客户端链路的目标框架由 `net6.0` 升级到 `net10.0`。
- 两个宿主项目已关闭 `PublishTrimmed`，避免反射、JSON、日志组件在发布时被裁剪后出现运行期异常。
- `log4net` 已从 `2.0.17` 升级到 `3.3.1`。
- 隧道传输缓冲区由原先的 `1GB` 下调到 `64KB`，并切换为池化复用。
- 热路径传输循环已经使用 `ArrayPool<byte>`、`Memory<byte>`、`Span<byte>` 进行低分配处理。
- 长度头、控制帧和小包编码逻辑改为 `BinaryPrimitives`，减少 `BitConverter` 和临时数组分配。
- 协议中的定长读取逻辑已调整为“必须读满指定长度”，不再依赖单次 `ReadAsync` 的偶然完整返回。
- 压缩路径已做一轮重点优化：
  - 复用线程本地 `SnappyCompressor` / `SnappyDecompressor`
  - 压缩包读取和写入使用租用缓冲区
  - 避免在主传输循环中为每个压缩包额外构造 `CompressedBytes` 中间对象
- 加密和证书加载 API 已同步调整为更适合 `.NET 10` 的现代实现。

## 产物目录

- Linux 自包含服务端：
  - `publish/nspserver-linux-x64-sc-net10`
- Windows 自包含客户端：
  - `publish/nspclient-win-x64-sc-net10`

## 验证方式

- `dotnet build src/NSmartProxy.ServerHost/NSmartProxy.ServerHost.csproj -c Release`
- `dotnet build src/NSmartProxyClient/NSmartProxyClient.csproj -c Release`
- `dotnet publish ... linux-x64 --self-contained true`
- `dotnet publish ... win-x64 --self-contained true`

截至本轮收尾，主链路服务端和客户端的 `Release` 构建均已通过，并且验证路径上的构建告警已清零。

## 上线说明

- 用新的 `net10` 自包含发布目录替换现有二进制文件。
- `appsettings.json` 和现有运行配置原则上可以沿用，除非你本次准备顺手调整端口、鉴权或映射策略。
- 如果线上目录里已经有日志、本地数据库或其他运行时文件，替换二进制前建议先备份。
- 本轮对压缩路径做的是“内存分配优化”，没有修改现有线协议格式，因此压缩兼容性保持不变。

## 后续建议

- 建议增加基础压测或 benchmark，至少覆盖这几类场景：
  - 压缩与未压缩模式的吞吐对比
  - SSE / 流式响应的长连接稳定性
  - 高频短连接 HTTP 请求的抖动和延迟
- 如果后面还要继续做性能优化，下一阶段最值得看的不是外层隧道循环，而是 Snappy 库内部实现本身。
