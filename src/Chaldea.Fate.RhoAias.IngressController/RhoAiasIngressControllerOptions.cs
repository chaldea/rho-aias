namespace Chaldea.Fate.RhoAias.IngressController;

public class RhoAiasIngressControllerOptions
{
	public bool Enable { get; set; } = false;
	public string ControllerClass { get; set; } = "chaldea.cn/ingress-rho-aias";
	public bool ServerCertificates { get; set; } = false;
	public string DefaultSslCertificate { get; set; } = "rho-aias/rho-aias-ingress-tls";
	public string ControllerServiceName { get; set; } = "ingress-rho-aias-controller";
	public string ControllerServiceNamespace { get; set; } = "rho-aias";
}