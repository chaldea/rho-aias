namespace Chaldea.Fate.RhoAias.IngressController;

public class RhoAiasIngressControllerOptions
{
	public bool Enable { get; set; } = false;
	public string ControllerClass { get; set; } = "microsoft.com/ingress-yarp";
	public bool ServerCertificates { get; set; } = false;
	public string DefaultSslCertificate { get; set; } = "yarp/yarp-ingress-tls";
	public string ControllerServiceName { get; set; } = "ingress-yarp-controller";
	public string ControllerServiceNamespace { get; set; } = "yarp";
}