using MessagePack;

namespace Chaldea.Fate.RhoAias;

public enum ProxyType
{
    HTTP,
    HTTPS,
    TCP,
    UDP,
}

[MessagePackObject]
public class Client
{
    [Key(0)] public Guid Id { get; set; }
    [Key(1)] public string Name { get; set; }
    [Key(2)] public string Version { get; set; }
    [Key(3)] public string Token { get; set; }
    [Key(4)] public string Endpoint { get; set; }
    [Key(5)] public string ConnectionId { get; set; }
    [Key(6)] public Proxy[] Proxies { get; set; }
}
