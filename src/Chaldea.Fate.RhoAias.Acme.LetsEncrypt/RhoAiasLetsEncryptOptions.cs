namespace Chaldea.Fate.RhoAias.Acme.LetsEncrypt;

public class RhoAiasLetsEncryptOptions
{
    public string CertRootDirectory { get; set; } = "certs";
    public string CountryName { get; set; } = "CN";
    public string State { get; set; } = "Shanghai";
    public string Locality { get; set; } = "Shanghai";
    public string Organization { get; set; } = "Chaldea";
    public string OrganizationUnit { get; set; } = "Development";
}