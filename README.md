<img src="NSmartProxyNew.png" alt="NSmartProxy" width="420">

# NSmartProxy

[中文](./README.md) | [English](./README_EN.md) | [Server Guide](./README_SERVER.md) | [服务端说明](./README_SERVER_CN.md)

> 说明：本仓库基于原项目 [tmoonlight/NSmartProxy](https://github.com/tmoonlight/NSmartProxy) fork 后继续维护和修改。

`NSmartProxy` 是一个基于“反向连接池 + 服务端统一转发”的内网穿透工具。

它的工作方式不是 P2P 打洞，而是：

1. 内网客户端主动连接公网服务端
2. 服务端维护每个映射应用的待命反向连接
3. 外部用户访问公网端口时，服务端取出一条反向连接
4. 客户端再去连接本地真实服务，双方开始双向转发

当前仓库已经整理到更适合实际部署的状态：

- 主链路已升级到 `.NET 10`
- 推荐直接使用自包含发布产物
- 服务端和客户端都按普通控制台程序运行
- 不再依赖旧的 Windows Service 宿主封装

## 当前建议

- 如果你要穿透本地 API、网页、`new-api`、OpenAI 兼容接口，优先使用 `Protocol: TCP`
- 如果你还需要域名、HTTPS、反向代理、SSE 友好行为，建议在公网服务器前面再挂 `Nginx` 或 `Caddy`
- 仓库里的 `HTTP` 模式仍然保留，但更适合简单按 `Host` 分流，不建议承担复杂网关职责

## 仓库结构

- `src/NSmartProxy.ServerHost`: 服务端宿主
- `src/NSmartProxyClient`: 客户端宿主
- `src/NSmartProxy`: 服务端核心逻辑
- `src/NSmartProxy.ClientRouter`: 客户端路由与转发逻辑
- `src/NSmartProxy.Infrastructure`: 公共基础设施
- `publish/`: 本地发布产物目录

## 快速开始

下面是一套当前最推荐、也是本次联调验证过的最小用法。

### 1. 启动服务端

服务端配置文件示例，见 [`src/NSmartProxy.ServerHost/appsettings.json`](</D:/Work/QuestionByAi/NSmartProxy/src/NSmartProxy.ServerHost/appsettings.json>)：

```json
{
  "ReversePort": 7842,
  "ConfigPort": 7841,
  "WebAPIPort": 12309,
  "ReversePort_Out": 0,
  "ConfigPort_Out": 0
}
```

含义：

- `ReversePort`: 客户端反向连接接入端口
- `ConfigPort`: 客户端配置与心跳端口
- `WebAPIPort`: Web 管理端口
- `*_Out`: 服务端存在额外端口映射时可选配置，默认 `0` 即使用本机端口

Linux 运行方式：

```bash
chmod +x ./NSmartProxy.ServerHost
./NSmartProxy.ServerHost
```

Windows 运行方式：

```powershell
.\NSmartProxy.ServerHost.exe
```

启动后访问：

```text
http://<server-ip>:12309
```

如果你使用的是已有数据库，请以现有用户体系为准。

### 2. 启动客户端

客户端配置文件示例，见 [`src/NSmartProxyClient/appsettings.json`](</D:/Work/QuestionByAi/NSmartProxy/src/NSmartProxyClient/appsettings.json>)。

下面给一个更贴近当前推荐用法的最小配置：

```json
{
  "ProviderWebPort": 12309,
  "ProviderAddress": "your-server-ip",
  "Clients": [
    {
      "IP": "127.0.0.1",
      "TargetServicePort": 3000,
      "ConsumerPort": 52999,
      "Protocol": "TCP",
      "IsCompress": false,
      "Description": "local api"
    }
  ]
}
```

含义：

- `ProviderAddress`: 公网服务端地址
- `ProviderWebPort`: 服务端 Web/API 管理端口
- `TargetServicePort`: 内网真实服务端口
- `ConsumerPort`: 对外暴露端口
- `Protocol`: 推荐填 `TCP`
- `IsCompress`: 调试阶段建议先关掉，确认链路无误后再评估是否开启

Windows 运行方式：

```powershell
.\NSmartProxyClient.exe
```

如需显式登录：

```powershell
.\NSmartProxyClient.exe -u admin -p admin123
```

Linux 或 macOS 运行方式：

```bash
chmod +x ./NSmartProxyClient
./NSmartProxyClient
```

### 3. 验证链路

假设客户端把本地 `127.0.0.1:3000` 映射到了公网 `52999`，那么外部访问：

```text
http://<server-ip>:52999
```

如果你映射的是 HTTP API，本地直连和公网访问应返回一致的状态码、响应头和响应体语义。

## 推荐部署方式

对于当前仓库代码状态，最推荐的部署组合是：

```text
外部用户
  -> 公网服务器
  -> Nginx / Caddy
  -> NSmartProxy 服务端
  -> NSmartProxy 客户端
  -> 本地真实服务
```

尤其是大模型中转站、OpenAI 兼容 API、SSE 流式响应场景，建议：

- `NSmartProxy` 只做 `TCP` 穿透
- `Nginx` / `Caddy` 负责 `80/443`、TLS、域名和反向代理

## 构建与发布

本地构建：

```powershell
dotnet build .\src\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c Release
dotnet build .\src\NSmartProxyClient\NSmartProxyClient.csproj -c Release
```

自包含发布：

```powershell
dotnet publish .\src\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c Release -r linux-x64 --self-contained true
dotnet publish .\src\NSmartProxyClient\NSmartProxyClient.csproj -c Release -r win-x64 --self-contained true
```

## 相关文档

- 变更记录：[CHANGELOG.md](</D:/Work/QuestionByAi/NSmartProxy/CHANGELOG.md>)
- `.NET 10` 升级与优化说明：[docs/2026-04-29-net10-upgrade-and-optimization.md](</D:/Work/QuestionByAi/NSmartProxy/docs/2026-04-29-net10-upgrade-and-optimization.md>)
- 原理与优化评审：[docs/2026-04-29-nsmartproxy-principle-and-optimization-review.md](</D:/Work/QuestionByAi/NSmartProxy/docs/2026-04-29-nsmartproxy-principle-and-optimization-review.md>)

## 当前已知边界

- 服务端仍然存在一些“正常取消被记录为未观察异常”的日志噪音
- `HTTP` 模式适合简单场景，不建议替代专业 Web 网关
- 复杂 API 穿透优先用 `TCP`，并在前面加标准反向代理

## 许可证

项目许可证见 [LICENSE](</D:/Work/QuestionByAi/NSmartProxy/LICENSE>)。
