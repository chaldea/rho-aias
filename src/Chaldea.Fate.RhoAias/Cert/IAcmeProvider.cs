using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias;

public interface IAcmeProvider
{
    Task<CertInfo> CreateCertAsync(Cert cert);
    Task<byte[]> ReadCertFileAsync(string fileName);
}

internal class SelfSignedAcmeProvider : IAcmeProvider
{
    private CertOptions _options;

    public SelfSignedAcmeProvider(IOptions<CertOptions> options)
    {
        _options = options.Value;
    }

    public async Task<CertInfo> CreateCertAsync(Cert cert)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            $"CN=RhoAias",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                false));
        request.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        // Create self-signed cert
        var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddMonths(3));

        var certPath = Utilities.EnsurePath(AppContext.BaseDirectory, _options.CertRootDirectory);
        var pfx = certificate.Export(X509ContentType.Pfx, _options.CertPassword);

        var certFile = cert.GetFileName();
        var certFullPath = Path.Combine(certPath, certFile);
        if (File.Exists(certFullPath)) File.Delete(certFullPath);
        await File.WriteAllBytesAsync(certFullPath, pfx);

        return new CertInfo
        {
            File = certFile,
            Password = _options.CertPassword,
            CountryName = _options.CountryName,
            State = _options.State,
            Locality = _options.Locality,
            Organization = _options.Organization,
            OrganizationUnit = _options.OrganizationUnit,
            CommonName = cert.Domain
        };
    }

    public async Task<byte[]> ReadCertFileAsync(string fileName)
    {
        var path = Utilities.EnsurePath(AppContext.BaseDirectory, _options.CertRootDirectory);
        var certPath = Path.Combine(path, fileName);
        if (File.Exists(certPath)) return await File.ReadAllBytesAsync(certPath);

        return Array.Empty<byte>();
    }
}
