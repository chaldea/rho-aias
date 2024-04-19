using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;

namespace Chaldea.Fate.RhoAias;

public interface IMetrics
{
	IDictionary<string, object> GetMetrics();
	void UpDownClientTotal(int count);
	void UpDownClientOnline(int count);
}

internal class Metrics : IMetrics
{
	// client metrics
	private readonly ObservableGauge<int> _clientGauge;
	private int _clientTotal = 0;
	private int _clientOnline = 0;

	// network traffic
	private readonly ObservableGauge<long> _trafficTotalGauge;
	private readonly ObservableGauge<float> _trafficSecGauge;
	private readonly PerformanceCounter _trafficInSec;
	private readonly PerformanceCounter _trafficOutSec;
	private readonly PerformanceCounter _trafficTotalSec;

	// network connections
	private readonly ObservableGauge<int> _connectionGauge;

	// system
	private readonly ObservableGauge<float> _systemGauge;
	private readonly PerformanceCounter _cpuCounter;
	private readonly PerformanceCounter _memCounter;
	private readonly NetworkInterface? _networkInterface;

	public Metrics(IMeterFactory meterFactory)
	{
		var instance = GetNetworkInterfaceName();
		_networkInterface = GetNetworkInterface(instance);
		_trafficInSec = new("Network Interface", "Bytes Received/sec", instance);
		_trafficOutSec = new("Network Interface", "Bytes Sent/sec", instance);
		_trafficTotalSec = new("Network Interface", "Bytes Total/sec", instance);
		_cpuCounter = new("Processor", "% Idle Time", "_Total");
		_memCounter = new("Memory", "% Committed Bytes In Use");

		var meter = meterFactory.Create("RhoAias");
		_clientGauge = meter.CreateObservableGauge("clients", GetClients);
		_trafficTotalGauge = meter.CreateObservableGauge("traffic.total", GetNetworkTrafficTotal);
		_trafficSecGauge = meter.CreateObservableGauge("traffic.sec", GetNetworkTrafficSec);
		_connectionGauge = meter.CreateObservableGauge("connection", GetNetworkConnections);
		_systemGauge = meter.CreateObservableGauge("system", GetSystemUsage);
	}

	public IDictionary<string, object> GetMetrics()
	{
		var metrics = new Dictionary<string, object>();
		foreach (var client in GetClients())
		{
			metrics[$"client_{client.Tags[0].Value}"] = client.Value;
		}

		foreach (var traffic in GetNetworkTrafficTotal())
		{
			metrics[$"traffic_{traffic.Tags[0].Value}"] = traffic.Value;
		}

		foreach (var traffic in GetNetworkTrafficSec())
		{
			metrics[$"traffic_{traffic.Tags[0].Value}"] = traffic.Value;
		}

		foreach (var connection in GetNetworkConnections())
		{
			metrics[$"connection_{connection.Tags[0].Value}"] = connection.Value;
		}

		foreach (var system in GetSystemUsage())
		{
			metrics[$"system_{system.Tags[0].Value}"] = system.Value;
		}

		return metrics;
	}

	public void UpDownClientTotal(int count)
	{
		_clientTotal += count;
	}

	public void UpDownClientOnline(int count)
	{
		_clientOnline += count;
	}

	private List<Measurement<int>> GetClients()
	{
		return new List<Measurement<int>>
		{
			new(_clientTotal, new KeyValuePair<string, object?>("status", "all")),
			new(_clientOnline, new KeyValuePair<string, object?>("status", "online"))
		};
	}

	private List<Measurement<long>> GetNetworkTrafficTotal()
	{
		var totalIn = 0L;
		var totalOut = 0L;
		if (_networkInterface != null)
		{
			var s = _networkInterface.GetIPv4Statistics();
			totalIn = s.BytesReceived;
			totalOut = s.BytesSent;
		}
		return new List<Measurement<long>>
		{
			new(totalIn, new KeyValuePair<string, object?>("type", "in_total")),
			new(totalOut, new KeyValuePair<string, object?>("type", "out_total")),
		};
	}

	private List<Measurement<float>> GetNetworkTrafficSec()
	{
		return new List<Measurement<float>>
		{
			new(_trafficInSec.NextValue(), new KeyValuePair<string, object?>("type", "in_sec")),
			new(_trafficOutSec.NextValue(), new KeyValuePair<string, object?>("type", "out_sec")),
			new(_trafficTotalSec.NextValue(), new KeyValuePair<string, object?>("type", "total_sec")),
		};
	}

	private List<Measurement<int>> GetNetworkConnections()
	{
		var properties = IPGlobalProperties.GetIPGlobalProperties();
		var tcpConnections = properties.GetActiveTcpConnections().Length;
		var udpConnections = properties.GetActiveUdpListeners().Length;

		return new List<Measurement<int>>
		{
			new(tcpConnections, new KeyValuePair<string, object?>("type", "tcp")),
			new(udpConnections, new KeyValuePair<string, object?>("type", "udp"))
		};
	}

	private List<Measurement<float>> GetSystemUsage()
	{
		var idle = _cpuCounter.NextValue();
		var cpuUsage = idle == 0 ? 0 : 100 - idle;
		return new List<Measurement<float>>
		{
			new(cpuUsage, new KeyValuePair<string, object?>("label", "cpu")),
			new(_memCounter.NextValue(), new KeyValuePair<string, object?>("label", "memory"))
		};
	}

	private string GetNetworkInterfaceName()
	{
		var pcg = new PerformanceCounterCategory("Network Interface");
		var names = pcg.GetInstanceNames();
		string name = null;
		foreach (var n in names)
		{
			if (Regex.IsMatch(n, @"\b(Wi-Fi|USB|Bluetooth)\b", RegexOptions.IgnoreCase))
			{
				continue;
			}
			name = n;
			break;
		}
		if (name == null)
		{
			name = names[0];
		}
		return name;
	}

	private NetworkInterface? GetNetworkInterface(string name)
	{
		var interfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (var ni in interfaces)
		{
			if (ni.Description == name)
			{
				return ni;
			}
		}

		return null;
	}
}