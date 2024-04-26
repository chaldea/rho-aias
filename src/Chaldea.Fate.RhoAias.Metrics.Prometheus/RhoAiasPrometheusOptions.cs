namespace Chaldea.Fate.RhoAias.Metrics.Prometheus;

public class RhoAiasPrometheusOptions
{
	// default Meters
	public string[] Meters { get; set; } = new[]
	{
		"RhoAias",
		// "System.Net.Http",
		// "System.Net.Sockets",
		// "Microsoft.AspNetCore.Hosting",
		// "Microsoft.AspNetCore.Server.Kestrel"
	};
}