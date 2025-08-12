namespace Chaldea.Fate.RhoAias;

public enum AcmeStatus
{
    NotIssued = 0,
    Issued,
    IssueFailed
}

public enum CertType
{
    SubDomain = 0,
    WildcardDomain = 1
}

public class Cert
{
    public Guid Id { get; set; }

    public CertType CertType { get; set; }

    public string Domain { get; set; } = default!;

    public DateTime? Expires { get; set; }

    public string Issuer { get; set; } = default!;

    public string Email { get; set; } = default!;

    public AcmeStatus Status { get; set; }

    public Guid? DnsProviderId { get; set; }

    public DnsProvider? DnsProvider { get; set; }

    public CertInfo? CertInfo { get; set; }

    public Cert UpdateExpires()
    {
        Expires = DateTime.UtcNow.AddMonths(3);
        return this;
    }

    public Cert UpdateCertInfo(CertInfo? certInfo)
    {
        CertInfo = certInfo;
        return this;
    }

    public Cert UpdateStatus(AcmeStatus status)
    {
        Status = status;
        return this;
    }

    public string TrimDomain()
    {
        return Domain.Replace("*.", "");
    }

    public string GetFileName()
    {
        return $"{TrimDomain()}.pfx";
    }

    public bool IsWildcardDomain()
    {
        return Domain.StartsWith("*.");
    }
}

public class CertInfo
{
    public string File { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string CountryName { get; set; } = default!;
    public string State { get; set; } = default!;
    public string Locality { get; set; } = default!;
    public string Organization { get; set; } = default!;
    public string OrganizationUnit { get; set; } = default!;
    public string CommonName { get; set; } = default!;
}
