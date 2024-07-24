namespace Chaldea.Fate.RhoAias;

public interface IAcmeProvider
{
    Task<CertInfo> CreateCertAsync(Cert cert);
    Task<byte[]> ReadCertFileAsync(string fileName);
}
