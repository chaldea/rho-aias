# RhoAias

RhoAias(/ˈroʊ/ - /ˈaɪ.əs/) is a library for reverse proxy and intranet traversal, that can be deployed directly as a standalone application or embedded as a dependency library in your dotnet application.

## Features

- Support `HTTP` reverse proxy, which can forward requests based on the location level.
- The client can be started without configuration, and the configuration can be delivered from the server dashboard
- Support `TCP`/`UDP` port forwarding, which can be SSH connected to the private network
- Support k8s-ingress. The client can be deployed as a k8s-ingress so that ingress traffic can be forwarded to the intranet k8s cluster.
- Support ACME-based HTTPS certificate and certificate renewal
- Support `Metrics` monitoring, which can be connected to OpenTelemetry-based monitoring tools, such as `Prometheus`
- Support data stream compression (compression algorithm supports gzip, snappy, etc.)
- Support `WAF`(web application firewall), which can defend against script and bot attacks

## Usage Scenario

RhoAias can be used in many scenarios, and the following is a set of use case diagrams to quickly understand the use cases of RhoAias.

#### 1.HTTP request forwarding

Forward all HTTP requests from the public network to the intranet by the paths.

![](docs/imgs/http-forwarding.svg)

#### 2.Multiple ENV forwarding

RhoAias can forward requests to different `ENV` servers by the paths.

![](docs/imgs/multiple-env.svg)

#### 3.Load Balancing

RhoAias supports load balancing across DCs or regions.

![](docs/imgs/load-balancing.svg)

**NOTE: Load balancing for a single client doesn't make sense.**

#### 4.K8S-Ingress

RhoAias client can be deployed as a k8s-ingress.

![](docs/imgs/k8s-ingress.svg)

#### 5.Port forwarding

RhoAias can expose internal network ports to the public.

![](docs/imgs/port-forwarding.svg)

## Get Started

### Server Deployment

You need to prepare a machine with a public IP address and make sure that the machine has the Docker environment installed.

```yml
services:
  rhoaias-server:
    container_name: rhoaias-server
    image: chaldea/rhoaias-server
    restart: always
    network_mode: host
    environment:
      RhoAias__Server__Bridge: 8024 # Client connection port
      RhoAias__Server__Http: 80 # HTTP request forwarding port
      RhoAias__Server__Https: 443 # HTTPS request forwarding port
    volumes:
      - rhoaias_server_data:/app/data # Data store directory
      - rhoaias_server_certs:/app/certs # HTTPS cert store directory

volumes:
  rhoaias_server_data:
  rhoaias_server_certs:
```

Due to port forwarding requires listening on any port, so you need to enable the host network mode like `network_mode: host`.

Save that configuration to `docker-compose.yml`,then exec

```sh
docker compose up -d
```

Determine that the ports on the docker container are reachable, If the port is unreachable, check your firewall configuration.

**Server Environment Variables**

| Variable Name                               | Default Value | Description                                                |
| ------------------------------------------- | ------------- | ---------------------------------------------------------- |
| RhoAias\_\_Server\_\_Bridge                 | 8024          | The client connection port, and the Dashboard access port. |
| RhoAias\_\_Server\_\_Http                   | 80            | HTTP request port.                                         |
| RhoAias\_\_Server\_\_Https                  | 443           | HTTPS request port.                                        |
| RhoAias\_\_Dashboard\_\_UserName            | admin         | Dashboard default username.                                |
| RhoAias\_\_Dashboard\_\_Password            | 123456Aa      | Dashboard default password.                                |
| RhoAias\_\_Dashboard\_\_CreateDefaultUser   | true          | Whether to create a default user.                          |
| RhoAias\_\_Dashboard\_\_CreateDefaultClient | true          | Whether to generate a default client key.                  |

### Generate Client Token Key

After the server starts, enter the Dashboard(http://{public-ip}:8024) with the default username and password. The dashboard will auto generate a default client key, which you can also manually create in the client list page.

![](docs/imgs/client-list.png)

### Client Deployment

RhoAias provides a variety of client deployment methods, such as docker, binary, k8s-ingress etc.

#### 1.Docker Mode

Create a docker configuration file `docker-compose.yml` on your intranet machine.

```yml
services:
  rhoaias-client:
    container_name: rhoaias-client
    image: chaldea/rhoaias-client
    restart: always
    environment:
      # your public server url address
      RhoAias__Client__ServerUrl: http://{server-ip}:8024
      # The token key that you created on the Dashboard
      RhoAias__Client__Token: PCv11vMiZkigHfnzcMLTFg
```

Then execute the command and ensure the client's status on the server dashboard page is online.

```sh
docker compose up -d
```

**Client Environment Variables**

| Variable Name                  | Description         |
| ------------------------------ | ------------------- |
| RhoAias\_\_Client\_\_ServerUrl | Server url address. |
| RhoAias\_\_Client\_\_Token     | Client `TokenKey`   |

#### 2.Binary Service Mode(Optional)

You can download the client binary program on the [Release](https://github.com/chaldea/rho-aias/releases) page.

Runs as console:

```sh
rhoaias-client -s http://{server-ip}:8024 -t PCv11vMiZkigHfnzcMLTFg
```

| Startup Parameters | Description         |
| ------------------ | ------------------- |
| -s, --server       | Server url address. |
| -t, --token        | Client `TokenKey`   |


If you need to run the client as a system service:

* On the windows, you can use [nssm](https://nssm.cc/usage)
* On the linux, you can use systemd

#### 3.K8S-Ingress Mode(Optional)

In your K8s cluster, create a namespace:
```bash

```


