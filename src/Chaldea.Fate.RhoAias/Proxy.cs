using System.Text.Json;
using MessagePack;
using Yarp.ReverseProxy.Configuration;

namespace Chaldea.Fate.RhoAias;

public enum ProxyType
{
	HTTP = 1,
	[Obsolete("Use http only.")]
	HTTPS = 2,
	TCP = 3,
	UDP = 4,
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
	[Key(8)] public string? Destination { get; set; }
	[Key(9)] public string? RouteConfig { get; set; }
    [Key(10)] public string? ClusterConfig { get; set; }
	[IgnoreMember] public Guid ClientId { get; set; }
	[IgnoreMember] public Client? Client { get; set; }

	public Proxy()
	{
	}

	public Proxy(RouteConfig route, ClusterConfig cluster)
	{
		var item = cluster.Destinations.FirstOrDefault();
		Id = Guid.NewGuid();
		Name = route.ClusterId;
		Type = ProxyType.HTTP;
		LocalIP = string.Empty;
		Path = route.Match.Path;
		Hosts = route.Match.Hosts?.ToArray();
		Destination = item.Value.Address;
		RouteConfig = JsonSerializer.Serialize(route);
		ClusterConfig = JsonSerializer.Serialize(cluster);
	}

	public string GetSchema() => Type switch
    {
        ProxyType.HTTP => "http",
        ProxyType.HTTPS => "http",
        ProxyType.TCP => "tcp",
        ProxyType.UDP => "udp",
        _ => string.Empty
    };

    public string GetUrl()
    {
	    if (string.IsNullOrEmpty(Destination))
	    {
		    return $"{GetSchema()}://{LocalIP}:{LocalPort}";
		}

	    return Destination;
    }

    public string GetHosts()
    {
        return string.Join(",", Hosts);
    }

    public void Update(Proxy proxy)
    {
        Name = proxy.Name;
        Type = proxy.Type;
        LocalIP = proxy.LocalIP;
        LocalPort = proxy.LocalPort;
        RemotePort = proxy.RemotePort;
        Path = proxy.Path;
        Hosts = proxy.Hosts;
        Destination = proxy.Destination;
        RouteConfig = proxy.RouteConfig;
        ClusterConfig = proxy.ClusterConfig;
    }

    public void UpdateLocalIp()
    {
	    if (string.IsNullOrEmpty(LocalIP) && !string.IsNullOrEmpty(Destination))
	    {
            var uri = new Uri(Destination);
            LocalIP = uri.Host;
            LocalPort = uri.Port;
	    }
    }
}