# Rho-Aias
Rho-Aias是一个用于反向代理和内网穿透的工具库，它既可以作为独立应用直接部署，同时也可以作为依赖库嵌入到当前应用中。

## Rho-Aias特性

* 支持反向代理，不但支持http转发，同时支持location级别的内网转发。也支持跨多个客户端的负载均衡
* 支持无配置启动，客户端无需配置可以直接启动，在服务端的Dashboard可以动态配置转发规则。
* 支持端口转发，除http协议转发之外，也支持端口转发。可实现ssh连接内网。
* 客户端支持k8s-ingress，可以直接将公网流量转发至内网的k8s集群。
* 支持申请的基于ACME协议的https证书，支持证书自动续期。
* 支持客户端jwt认证。
* 支持接入Prometheus等监控。基于OpenTelemetry标准。

## 使用场景
### Http请求转发
将所有公网的http请求依据path路径转发至内网对应的服务上。


![http](docs/web-server.svg)

### K8S-Ingress转发
将所有公网请求转发至内网的指定的k8s集群。

![k8s-ingress](docs/k8s-ingress.svg)


## 开始使用
### 安装Rho-Aias服务端程序
你需要有一台公网机器。确保该机器已经安装docker环境。
```yml
version: '3.7'
services:
  rhoaias-server:
    container_name: rhoaias-server
    image: chaldea/rhoaias-server
    restart: always
    network_mode: host
    environment:
      RhoAias__Server__Bridge: 8024  # 客户端连接端口
      RhoAias__Server__Http: 80      # http请求转发端口
      RhoAias__Server__Https: 443    # https请求转发端口
    volumes:
      - rhoaias_server_data:/app/data    # 数据存储目录
      - rhoaias_server_certs:/app/certs  # https证书存储目录

volumes:
  rhoaias_server_data:
  rhoaias_server_certs:
```
复制以上配置到docker-compose.yml文件中。执行指令：
```sh
docker compose up -d
```

### 创建客户端Token
服务启动后，打开Dashboard页面。http://{公网IP或域名}:8024。输入Dashboard用户名和密码，默认用户名admin密码为123456Aa。进入Dashboard在客户端列表页面中新建客户端，并复制Token。
![client-token](docs/client-token.png)

### 启动客户端程序
在公司内网机器上，创建如下启动配置：
```yml
version: "3.7"
services:
  rhoaias-client:
    container_name: rhoaias-client
    image: chaldea/rhoaias-client
    restart: always
    environment:
      # 公网IP或域名，确保8024端口可以正常访问
      RhoAias__Client__ServerUrl: http://rhoaias-server:8024
      # 创建客户端时生成的Token
      RhoAias__Client__Token: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9....
```
执行一下指定启动客户端。在Dashboard中查看客户端状态为在线，即表示客户端连接成功。
```sh
docker compose up -d
```

### 创建转发规则
在Dashboard的转发列表中，创建http转发，即可将指定的请求转发至内网指定的服务上。

![forwards](docs/forward.png)

### Https证书申请
对于Http网站，通常都需要Https证书。Rho-Aias支持ACME免费https证书。只需要在证书管理页面申请即可。


其中颁发机构LetsEncrypt支持单域名(a.sample.com)和泛域名(*.sample.com)证书。其中泛域名证书需要通过DNS服务商验证。因此需要提供DNS服务商配置。

## 嵌入应用
Rho-Aias既可以作为独立应用部署，同时可以直接嵌入当前应用中。

### 通过nuget包安装
```sh
dotnet add package Chaldea.Fate.RhoAias
```

| Nuget包                                       | 说明                                                          |
| --------------------------------------------- | ------------------------------------------------------------- |
| Chaldea.Fate.RhoAias                          | 核心包，如果只需要穿透功能，只安装该包即可                    |
| Chaldea.Fate.RhoAias.Authentication.JwtBearer | jwt认证包，客户端连接授权认证，如果已有自己idserver，可以省去 |
| Chaldea.Fate.RhoAias.Repository.Sqlite        | 仓储实现，默认数据存在内存，持久化需要实现IRepository接口     |
| Chaldea.Fate.RhoAias.Metrics.Prometheus       | Metric提供器，对外提供Metric数据接口                          |
| Chaldea.Fate.RhoAias.Dashboard                | Dashboard管理程序                                             |
| Chaldea.Fate.RhoAias.Acme.LetsEncrypt         | ACME证书提供器                                                |

具体开发可以参考[开发文档]()


## 贡献
- 如果遇到bug可以直接提交至dev分支
- 使用遇到问题可以通过issues反馈
- 项目处于开发阶段，还有很多待完善的地方，如果可以贡献代码，请提交 PR 至 dev 分支
- 如果有新的功能特性反馈，可以通过issues或者qq群反馈