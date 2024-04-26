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
public class Proxy
{
    [Key(0)] public Guid Id { get; set; }
    [Key(1)] public string Name { get; set; }
    [Key(2)] public ProxyType Type { get; set; }
    [Key(3)] public string LocalIP { get; set; }
    [Key(4)] public int LocalPort { get; set; }
    [Key(5)] public int RemotePort { get; set; }
    [Key(6)] public string? Path { get; set; }
    [Key(7)] public string[]? Hosts { get; set; }
    [IgnoreMember] public Guid ClientId { get; set; }
	[IgnoreMember] public Client? Client { get; set; }

    public string GetSchema() => Type switch
    {
        ProxyType.HTTP => "http",
        ProxyType.HTTPS => "https",
        ProxyType.TCP => "tcp",
        ProxyType.UDP => "udp",
        _ => string.Empty
    };

    public string GetUrl()
    {
        return $"{GetSchema()}://{LocalIP}:{LocalPort}";
    }

    public string GetHosts()
    {
        return string.Join(",", Hosts);
    }
}