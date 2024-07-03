# Tools

## SocketTester

A socket testing tool.

### Usage

```bash
cd SocketTester

# send tcp packages
dotnet run -- -t tcp -sp 9999 -cp 9999 -tp 10 -c

# send udp packages
dotnet run -- -t udp -sp 9999 -cp 9999 -tp 10 -c
```

Startup parameters

```bash
dotnet run -- -h
```

| Name              | Default Value | Desc                                           |
| ----------------- | ------------- | ---------------------------------------------- |
| --type,-t         | tcp           | Socket type, value is tcp,udp etc.             |
| --server-port,-sp | 9999          | Server listen port                             |
| --client-port,-cp | 8888          | Client connection port                         |
| --compressed,-c   | false         | Enable stream compression                      |
| --frequency,-f    | 1000          | Frequency of sending(ms)                       |
| --total-packs,-tp | 0             | Maximum number of packets sent. 0 is unlimited |
| --pack-size,-ps   | 1024          | Package size                                   |
