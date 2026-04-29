<img src="NSmartProxyNew.png" alt="NSmartProxy" width="420">

# NSmartProxy

[Chinese README](./README.md) | English | [Server Guide](./README_SERVER.md) | [服务端说明](./README_SERVER_CN.md)

> Note: this repository is a maintained fork of the original project [tmoonlight/NSmartProxy](https://github.com/tmoonlight/NSmartProxy).

`NSmartProxy` is an intranet tunneling tool built on a reverse connection pool and a public relay server.

It does not use P2P hole punching. The high-level flow is:

1. The client inside the private network connects out to the public server.
2. The server keeps standby reverse connections for each mapped app.
3. When an external user hits a public port, the server takes one standby reverse connection.
4. The client connects to the real local service and starts bidirectional forwarding.

## Current status

This repository has been modernized for current usage:

- The main path is upgraded to `.NET 10`
- Self-contained publishing is the recommended deployment option
- Both server and client run as plain console applications
- The old Windows service wrapper is no longer the recommended host model

## Recommended usage

- For local APIs, websites, `new-api`, or OpenAI-compatible endpoints, prefer `Protocol: TCP`
- For domains, HTTPS, reverse proxy features, or SSE-friendly behavior, put `Nginx` or `Caddy` in front of NSmartProxy
- The built-in `HTTP` mode still exists, but it is better treated as a simple host-based router rather than a full web gateway

## Repository layout

- `src/NSmartProxy.ServerHost`: server host
- `src/NSmartProxyClient`: client host
- `src/NSmartProxy`: server core
- `src/NSmartProxy.ClientRouter`: client routing and forwarding
- `src/NSmartProxy.Infrastructure`: shared infrastructure
- `publish/`: local publish outputs

## Quick start

### 1. Start the server

Example config: [`src/NSmartProxy.ServerHost/appsettings.json`](</D:/Work/QuestionByAi/NSmartProxy/src/NSmartProxy.ServerHost/appsettings.json>)

```json
{
  "ReversePort": 7842,
  "ConfigPort": 7841,
  "WebAPIPort": 12309,
  "ReversePort_Out": 0,
  "ConfigPort_Out": 0
}
```

Meaning:

- `ReversePort`: reverse connection port
- `ConfigPort`: config and heartbeat port
- `WebAPIPort`: web admin port
- `*_Out`: optional public-facing port override when another layer performs port translation

Linux:

```bash
chmod +x ./NSmartProxy.ServerHost
./NSmartProxy.ServerHost
```

Windows:

```powershell
.\NSmartProxy.ServerHost.exe
```

Then open:

```text
http://<server-ip>:12309
```

On a clean database, the server creates a default admin account: `admin / admin`.

### 2. Start the client

Example config: [`src/NSmartProxyClient/appsettings.json`](</D:/Work/QuestionByAi/NSmartProxy/src/NSmartProxyClient/appsettings.json>)

Minimal recommended config:

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

Windows:

```powershell
.\NSmartProxyClient.exe
```

Explicit login:

```powershell
.\NSmartProxyClient.exe -u admin -p admin123
```

Linux or macOS:

```bash
chmod +x ./NSmartProxyClient
./NSmartProxyClient
```

### 3. Verify the tunnel

If the client maps local `127.0.0.1:3000` to public `52999`, external traffic goes to:

```text
http://<server-ip>:52999
```

When tunneling an HTTP API, local direct access and public access should have the same semantics: status code, headers, and response meaning.

## Recommended deployment shape

For the current codebase, the most practical production shape is:

```text
External user
  -> Public server
  -> Nginx / Caddy
  -> NSmartProxy server
  -> NSmartProxy client
  -> Local target service
```

For LLM gateways and OpenAI-compatible APIs:

- Use `NSmartProxy` only for raw `TCP` tunneling
- Let `Nginx` or `Caddy` handle `80/443`, TLS, domain routing, and reverse proxy behavior

## Build and publish

Build:

```powershell
dotnet build .\src\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c Release
dotnet build .\src\NSmartProxyClient\NSmartProxyClient.csproj -c Release
```

Self-contained publish:

```powershell
dotnet publish .\src\NSmartProxy.ServerHost\NSmartProxy.ServerHost.csproj -c Release -r linux-x64 --self-contained true
dotnet publish .\src\NSmartProxyClient\NSmartProxyClient.csproj -c Release -r win-x64 --self-contained true
```

## Related documents

- Change log: [CHANGELOG.md](</D:/Work/QuestionByAi/NSmartProxy/CHANGELOG.md>)
- `.NET 10` upgrade and optimization notes: [docs/2026-04-29-net10-upgrade-and-optimization.md](</D:/Work/QuestionByAi/NSmartProxy/docs/2026-04-29-net10-upgrade-and-optimization.md>)
- Architecture and optimization review: [docs/2026-04-29-nsmartproxy-principle-and-optimization-review.md](</D:/Work/QuestionByAi/NSmartProxy/docs/2026-04-29-nsmartproxy-principle-and-optimization-review.md>)

## Known boundaries

- The server still has some log noise where normal cancellation surfaces as unobserved task exceptions
- Built-in `HTTP` mode is suitable for simple cases, not as a replacement for a full web gateway
- For complex APIs, prefer `TCP` mode and add a standard reverse proxy in front

## License

See [LICENSE](</D:/Work/QuestionByAi/NSmartProxy/LICENSE>).
