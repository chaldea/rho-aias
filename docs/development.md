# 开发文档

## 简单集成

以下为最简化的演示示例。

### 服务端开发

创建一个简单的服务端程序，服务端程序主要负责监听用户 http 请求或端口数据，并将此转发给客户端。

```bash
dotnet new webapi
```

添加包引用

```bash
dotnet add package Chaldea.Fate.RhoAias
```

打开 Program.cs 文件添加如下代码：

```csharp
// 注册服务
builder.Services.AddRhoAias(builder.Configuration);

app.UseRhoAias();
```

启动并运行

```bash
dotnet run
```

**说明** 程序运行后会监听端口，由于没有对 Kestrel 服务器做设置，默认 Http 监听和客户端连接转发都使用同一个端口。

添加初始化数据，可以使用 HostedService 来初始化，也可以直接在 Program 添加。

```csharp
var clientManager = app.Services.GetRequiredService<IClientManager>();
var proxyManager = app.Services.GetRequiredService<IProxyManager>();
// 创建默认客户端
var client = new Client
{
    Id = Guid.NewGuid(),
    Name = "Testing",
    Token = "1234567890"
};
await clientManager.CreateClientAsync(client);

// 创建一个转发配置
var proxy = new Proxy
{
    Id = Guid.NewGuid(),
    Name = "ForwardToClient",
    ClientId = client.Id,
    Type = ProxyType.HTTP,
    Hosts = new[] { "localhost:5008" },
    /*
     * 只把路径为/client的请求进行转发
     * 如果转发全部，那么当前程序的Controller接口就无法访问
     */
    Path = "/client/{**catch-all}",
    // 目标webapi应用，演示中目标webapi应用和客户端共用一个项目
    Destination = "http://localhost:5283"
};
proxy.EnsureLocalIp();
await proxyManager.CreateProxyAsync(proxy);
```

### 客户端开发

创建客户端程序，客户端程序主要负责接受服务器流转的数据包，并发送到指定的目标 IP 和端口上。

客户端程序可以依据需求使用不同模板创建，如果只是用来转发数据流可以使用控制台程序，也可以使用 webapi 程序。

```bash
dotnet new webapi
```

添加包引用

```bash
dotnet add package Chaldea.Fate.RhoAias
```

打开 Program.cs 文件添加如下代码：

```csharp
// 注册服务
builder.Services.AddRhoAiasClient(builder.Configuration);
```

添加配置到 appsetting.json

```json
"RhoAias": {
  "Client": {
    // 服务器地址
    "ServerUrl": "http://localhost:5008",
    // 创建Client时定义的Token
    "Token": "1234567890"
  }
}
```

启动并运行

```bash
dotnet run
```

上述客户端示例使用了一个 webapi 模板，主要是为了演示服务器可以转发指定路径的请求到目标 webapi 应用上。为了简化客户端直接和 webapi 项目合并为一个项目了。

在客户端的 Controllers 文件夹中创建 ClientController.cs 文件，添加如下代码：

```csharp
[ApiController]
public class ClientController : ControllerBase
{
    [HttpGet]
    [Route("/client/test")]
    public Task<string> TestAsync()
    {
        return Task.FromResult("Message from client.");
    }
}
```

验证结果，在浏览器中请求服务端的/client 接口，会全部转发到客户端的 api 上。

## 数据持久化

演示项目中，所有配置数据保存的内存中的，如果需要持久化，可以添加持久化的包到服务端项目中。

```bash
dotnet add package Chaldea.Fate.RhoAias.Repository.Sqlite
```

该包使用 Sqlite 作为持久化存储方案。你也可以通过创建自定义`Repository`类并实现`IRepository<TEntity>`接口来实现自定义存储。

可以引入EF来实现具体的数据库存储，示例：
```csharp
internal class MyRepository<TEntity>: IRepository<TEntity> where TEntity : class
{
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        await using var context = _myDbFactory.CreateDbContext();
        return await context.Set<TEntity>().AnyAsync(predicate);
    }
}
```

将存储替换成自定义存储类。
```csharp
builder.Services.Replace(new ServiceDescriptor(typeof(IRepository<>), typeof(MyRepository<>), ServiceLifetime.Singleton));
```
