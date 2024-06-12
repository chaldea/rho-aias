## 项目简介

该项目是一个最简单的演示项目。主要演示了Chaldea.Fate.RhoAias包的使用。

### 项目运行
启动服务端程序
```bash
dotnet run --project Simple.Server/Simple.Server.csproj
```

启动客户端程序
```bash
dotnet run --project Simple.Client/Simple.Client.csproj
```

### 验证代理效果
打开浏览器访问[http://localhost:5008/client/test](http://localhost:5008/client/test)

该接口访问地址是服务器端口，实际返回的是客户端接口的数据。这表示反向代理配置已经生效。

为了验证该库具有的穿透性，可以将客户端程序运行在服务端无法访问的子网段内。该测试接口仍能正常返回客户端接口的数据。
