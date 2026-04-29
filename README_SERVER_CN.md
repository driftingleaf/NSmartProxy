<img src="NSmartProxyNew.png" alt="NSmartProxy Server" width="420">

# NSmartProxy 服务端说明

[项目首页](./README.md) | [English README](./README_EN.md) | [Server Guide](./README_SERVER.md) | 服务端说明

> 说明：本仓库基于原项目 [tmoonlight/NSmartProxy](https://github.com/tmoonlight/NSmartProxy) fork 后继续维护和修改。

这份文档只聚焦 `NSmartProxy` 的公网服务端。

## 服务端负责什么

服务端是公网入口，主要职责有：

- 接收客户端建立的反向连接
- 监听对外暴露的消费端口
- 把外部请求匹配到具体映射应用
- 通知客户端回连本地真实服务
- 在服务端与客户端之间转发流量

## 当前运行方式

当前版本的服务端宿主已经是普通控制台程序。

推荐方式：

- Linux：直接运行自包含可执行文件
- Windows：直接运行自包含可执行文件

不再建议使用旧版本里那套 Windows Service 包装命令。

## 服务端配置

配置文件示例见 [`src/NSmartProxy.ServerHost/appsettings.json`](</D:/Work/QuestionByAi/NSmartProxy/src/NSmartProxy.ServerHost/appsettings.json>)：

```json
{
  "ReversePort": 7842,
  "ConfigPort": 7841,
  "WebAPIPort": 12309,
  "ReversePort_Out": 0,
  "ConfigPort_Out": 0
}
```

字段说明：

- `ReversePort`：客户端反向连接接入端口
- `ConfigPort`：配置与心跳端口
- `WebAPIPort`：Web 管理端口
- `ReversePort_Out`：可选的公网反向连接端口覆盖值
- `ConfigPort_Out`：可选的公网配置端口覆盖值

## 启动服务端

Linux：

```bash
chmod +x ./NSmartProxy.ServerHost
./NSmartProxy.ServerHost
```

Windows：

```powershell
.\NSmartProxy.ServerHost.exe
```

管理后台地址：

```text
http://<server-ip>:12309
```

如果是全新数据库，当前代码会创建默认管理员：

```text
admin / admin
```

## 常见端口

典型会用到这些端口：

- `7842`：客户端反向连接
- `7841`：配置与心跳
- `12309`：Web 管理端
- 动态消费端口，例如 `52999`：公网业务入口

如果你在服务端前面又套了一层端口映射或反向代理，可以按需配置 `*_Out`。

## 推荐部署形态

对 API 和网页服务，推荐：

```text
外部用户
  -> Nginx / Caddy
  -> NSmartProxy 服务端
  -> NSmartProxy 客户端
  -> 本地真实服务
```

尤其是 OpenAI 兼容 API、SSE 流式场景，建议：

- `NSmartProxy` 只跑 `TCP` 穿透
- `Nginx` / `Caddy` 负责域名、TLS 和反向代理行为

## 构建与发布

构建：

```powershell
dotnet build .\src\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c Release
```

发布 Linux 自包含产物：

```powershell
dotnet publish .\src\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c Release -r linux-x64 --self-contained true
```

## 相关文档

- 项目首页：[README.md](</D:/Work/QuestionByAi/NSmartProxy/README.md>)
- 升级说明：[docs/2026-04-29-net10-upgrade-and-optimization.md](</D:/Work/QuestionByAi/NSmartProxy/docs/2026-04-29-net10-upgrade-and-optimization.md>)
- 原理与优化评审：[docs/2026-04-29-nsmartproxy-principle-and-optimization-review.md](</D:/Work/QuestionByAi/NSmartProxy/docs/2026-04-29-nsmartproxy-principle-and-optimization-review.md>)
