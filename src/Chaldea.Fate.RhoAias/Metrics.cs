using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Chaldea.Fate.RhoAias;

public interface IMetrics
{
	IDictionary<string, object> GetMetrics();
	void UpDownClientTotal(int count, bool reset = false);
	void UpDownClientOnline(int count);
}

internal class Metrics : IMetrics
{
	private readonly NetworkInterface? _networkInterface;
	private readonly Stopwatch _sw = new();
	private readonly IMetricsCollector _collector;

	// client metrics
	private readonly ObservableGauge<int> _clientGauge;

	// network connections
	private readonly ObservableGauge<int> _connectionGauge;

	// system
	private readonly ObservableGauge<float> _systemGauge;
	private readonly ObservableGauge<float> _trafficSecGauge;

	// network traffic
	private readonly ObservableGauge<long> _trafficTotalGauge;
	private int _clientOnline;
	private int _clientTotal;
	private long _totalIn;

	private long _totalInOffset;
	private long _totalInOut;
	private long _totalInOutSec;
	private long _totalInSec;
	private long _totalOut;
	private long _totalOutOffset;
	private long _totalOutSec;

	public Metrics(IMeterFactory meterFactory)
	{
		_networkInterface = GetNetworkInterface();

		if (OperatingSystem.IsWindows())
		{
			_collector = new CollectorWindows();
		}
		else if (OperatingSystem.IsLinux())
		{
			_collector = new CollectorLinux();
		}
		else
		{
			_collector = new CollectorDefault();
		}

		// create metrics for OpenTelemetry (usage: eg Prometheus)
		var meter = meterFactory.Create("RhoAias");
		_clientGauge = meter.CreateObservableGauge("clients", GetClients);
		_trafficTotalGauge = meter.CreateObservableGauge("traffic.total", GetNetworkTrafficTotal);
		_trafficSecGauge = meter.CreateObservableGauge("traffic.sec", GetNetworkTrafficSec);
		_connectionGauge = meter.CreateObservableGauge("connection", GetNetworkConnections);
		_systemGauge = meter.CreateObservableGauge("system", GetSystemUsage);
	}

	// create metrics for self api
	public IDictionary<string, object> GetMetrics()
	{
		var metrics = new Dictionary<string, object>();
		foreach (var client in GetClients()) metrics[$"client_{client.Tags[0].Value}"] = client.Value;

		foreach (var traffic in GetNetworkTrafficTotal()) metrics[$"traffic_{traffic.Tags[0].Value}"] = traffic.Value;

		foreach (var traffic in GetNetworkTrafficSec()) metrics[$"traffic_{traffic.Tags[0].Value}"] = traffic.Value;

		foreach (var connection in GetNetworkConnections())
			metrics[$"connection_{connection.Tags[0].Value}"] = connection.Value;

		foreach (var system in GetSystemUsage()) metrics[$"system_{system.Tags[0].Value}"] = system.Value;

		return metrics;
	}

	public void UpDownClientTotal(int count, bool reset = false)
	{
		if (reset)
		{
			_clientTotal = count;
			return;
		}

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
		if (_networkInterface != null)
		{
			var s = _networkInterface.GetIPv4Statistics();
			var durationIn = s.BytesReceived - _totalInOffset;
			var durationOut = s.BytesSent - _totalOutOffset;
			if (_sw.IsRunning)
			{
				if (_sw.Elapsed.TotalSeconds >= 1)
				{
					_sw.Stop();
					_totalInSec = (long)((durationIn - _totalIn) / _sw.Elapsed.TotalSeconds);
					_totalOutSec = (long)((durationOut - _totalOut) / _sw.Elapsed.TotalSeconds);
					_totalInOutSec = (long)((durationIn + durationOut - _totalInOut) / _sw.Elapsed.TotalSeconds);
					_totalIn = durationIn;
					_totalOut = durationOut;
					_totalInOut = durationIn + durationOut;
					_sw.Restart();
				}
			}
			else
			{
				_totalIn = durationIn;
				_totalOut = durationOut;
				_totalInOut = durationIn + durationOut;
				_sw.Start();
			}
		}

		return new List<Measurement<long>>
		{
			new(_totalIn, new KeyValuePair<string, object?>("type", "in_total")),
			new(_totalOut, new KeyValuePair<string, object?>("type", "out_total"))
		};
	}

	private List<Measurement<float>> GetNetworkTrafficSec()
	{
		return new List<Measurement<float>>
		{
			new(_totalInSec, new KeyValuePair<string, object?>("type", "in_sec")),
			new(_totalOutSec, new KeyValuePair<string, object?>("type", "out_sec")),
			new(_totalInOutSec, new KeyValuePair<string, object?>("type", "total_sec"))
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
		return _collector.GetSystemUsage();
	}

	private NetworkInterface? GetNetworkInterface()
	{
		var interfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (var ni in interfaces)
		{
			if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
			    ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel ||
			    ni.OperationalStatus != OperationalStatus.Up)
				continue;
			if (Regex.IsMatch(ni.Name, @"\b(Wi-Fi|USB|Bluetooth|lo)\b", RegexOptions.IgnoreCase)) continue;
			InitCounter(ni);
			return ni;
		}

		return null;
	}

	private void InitCounter(NetworkInterface ni)
	{
		var s = ni.GetIPv4Statistics();
		_totalInOffset = s.BytesReceived;
		_totalOutOffset = s.BytesSent;
	}
}