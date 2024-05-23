using System.Text.Json;
using System.Text.Json.Serialization;
using Yarp.ReverseProxy.Configuration;

namespace Chaldea.Fate.RhoAias;

public enum ProxyType
{
	HTTP = 0,
	[Obsolete("Use http only.")]
	HTTPS = 1,
	TCP = 2,
	UDP = 3,
}

public class Proxy
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ProxyType Type { get; set; }
    public string LocalIP { get; set; } = default!;
    public int LocalPort { get; set; }
    public int RemotePort { get; set; }
    public string? Path { get; set; }
    public string[]? Hosts { get; set; }
	public string? Destination { get; set; }
	public string? RouteConfig { get; set; }
    public string? ClusterConfig { get; set; }
    public bool Disabled { get; set; }
	[JsonIgnore] public Guid ClientId { get; set; }
	[JsonIgnore] public Client? Client { get; set; }

	public Proxy()
	{
	}

	public Proxy(RouteConfig route, ClusterConfig cluster)
	{
		var item = cluster.Destinations?.FirstOrDefault();
		Id = Guid.NewGuid();
		Name = route.ClusterId ?? route.RouteId;
		Type = ProxyType.HTTP;
		LocalIP = string.Empty;
		Path = route.Match.Path;
		Hosts = route.Match.Hosts?.ToArray();
		Destination = item?.Value.Address;
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
	    return Hosts == null ? string.Empty : string.Join(",", Hosts);
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