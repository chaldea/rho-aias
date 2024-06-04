using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Connections;

namespace Chaldea.Fate.RhoAias;

public interface IServerCertificateSelector
{
    X509Certificate2? Select(ConnectionContext context, string? domainName);
}

internal class DefaultServerCertificateSelector : IServerCertificateSelector
{
    private readonly ICertManager _certManager;

    public DefaultServerCertificateSelector(ICertManager certManager)
    {
        _certManager = certManager;
    }

    public X509Certificate2? Select(ConnectionContext context, string? domainName)
    {
        return domainName == null ? null : _certManager.GetCert(domainName);
    }
}