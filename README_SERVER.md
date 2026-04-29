<img src="NSmartProxyNew.png" alt="NSmartProxy Server" width="420">

# NSmartProxy Server Guide

[Project README](./README.md) | [English README](./README_EN.md) | Server Guide | [服务端说明](./README_SERVER_CN.md)

> Note: this repository is a maintained fork of the original project [tmoonlight/NSmartProxy](https://github.com/tmoonlight/NSmartProxy).

This document focuses only on the public server side of NSmartProxy.

## What the server does

The server is the public entry point. It is responsible for:

- accepting reverse connections from clients
- listening on public consumer ports
- matching incoming traffic to a mapped app
- telling the client to connect back to the local target service
- forwarding traffic between both sides

## Runtime model

The current host is a plain console application.

Recommended startup:

- Linux: run the self-contained executable directly
- Windows: run the self-contained executable directly

Do not rely on the old Windows service wrapper commands from older releases.

## Server configuration

Example file: [`src/NSmartProxy.ServerHost/appsettings.json`](</D:/Work/QuestionByAi/NSmartProxy/src/NSmartProxy.ServerHost/appsettings.json>)

```json
{
  "ReversePort": 7842,
  "ConfigPort": 7841,
  "WebAPIPort": 12309,
  "ReversePort_Out": 0,
  "ConfigPort_Out": 0
}
```

Field summary:

- `ReversePort`: client reverse connection port
- `ConfigPort`: config and heartbeat port
- `WebAPIPort`: web management port
- `ReversePort_Out`: optional external reverse port override
- `ConfigPort_Out`: optional external config port override

## Start the server

Linux:

```bash
chmod +x ./NSmartProxy.ServerHost
./NSmartProxy.ServerHost
```

Windows:

```powershell
.\NSmartProxy.ServerHost.exe
```

Web console:

```text
http://<server-ip>:12309
```

On a clean database, the default admin account is:

```text
admin / admin
```

## Network ports

Typical exposed ports:

- `7842`: reverse connections from clients
- `7841`: config and heartbeat channel
- `12309`: web management
- dynamic consumer ports such as `52999`: public app entry ports

If you place another reverse proxy or port translation layer in front of NSmartProxy, configure `*_Out` as needed.

## Recommended production layout

For APIs and web services, use:

```text
External user
  -> Nginx / Caddy
  -> NSmartProxy server
  -> NSmartProxy client
  -> local target service
```

Especially for OpenAI-compatible APIs and SSE traffic:

- keep NSmartProxy in `TCP` mode
- let `Nginx` or `Caddy` handle domain, TLS, and reverse proxy behavior

## Build and publish

Build:

```powershell
dotnet build .\src\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c Release
```

Self-contained publish for Linux:

```powershell
dotnet publish .\src\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c Release -r linux-x64 --self-contained true
```

## Related docs

- Main project README: [README.md](</D:/Work/QuestionByAi/NSmartProxy/README.md>)
- Upgrade notes: [docs/2026-04-29-net10-upgrade-and-optimization.md](</D:/Work/QuestionByAi/NSmartProxy/docs/2026-04-29-net10-upgrade-and-optimization.md>)
- Architecture review: [docs/2026-04-29-nsmartproxy-principle-and-optimization-review.md](</D:/Work/QuestionByAi/NSmartProxy/docs/2026-04-29-nsmartproxy-principle-and-optimization-review.md>)
