using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chaldea.Fate.RhoAias;

public interface ICertManager
{
	Task CreateCertAsync(Cert cert);
	Task RemoveCertAsync(Guid id);
	Task<List<Cert>> GetCertListAsync();
	X509Certificate2? GetCert(string domain);
	Task IssueCertAsync(Guid id);
}

public interface IAcmeProvider
{
	Task<CertInfo> CreateCertAsync(Cert cert);
	Task<byte[]> ReadCertFileAsync(string fileName);
}

internal class CertManager : ICertManager
{
	private readonly ILogger<CertManager> _logger;
	private readonly IRepository<Cert> _certRepository;
	private readonly IRepository<DnsProvider> _dnsProviderRepository;
	private readonly IServiceProvider _serviceProvider;
	private readonly ConcurrentDictionary<string, Task> _certJobs = new();
	private readonly ConcurrentDictionary<string, X509Certificate2> _challengeCerts =
		new(StringComparer.OrdinalIgnoreCase);

	public CertManager(
		ILogger<CertManager> logger, 
		IRepository<Cert> certRepository,
		IRepository<DnsProvider> dnsProviderRepository,
		IServiceProvider serviceProvider)
	{
		_logger = logger;
		_certRepository = certRepository;
		_dnsProviderRepository = dnsProviderRepository;
		_serviceProvider = serviceProvider;
	}

	public async Task CreateCertAsync(Cert entity)
	{
		entity.Id = Guid.NewGuid();
		await _certRepository.InsertAsync(entity);
		if (_certJobs.ContainsKey(entity.Domain))
		{
			return;
		}
		_certJobs.TryAdd(entity.Domain, Task.Factory.StartNew(async() =>
		{
			await IssueCertAsync(entity);
		}));
	}

	public async Task IssueCertAsync(Guid id)
	{
		var entity = await _certRepository.GetAsync(x => x.Id == id);
		if (entity == null) return;
		if (_certJobs.ContainsKey(entity.Domain))
		{
			return;
		}
		_certJobs.TryAdd(entity.Domain, Task.Factory.StartNew(async () =>
		{
			await IssueCertAsync(entity);
		}));
	}

	public async Task<List<Cert>> GetCertListAsync()
	{
		var list = await _certRepository.GetListAsync();
		return list;
	}

	public X509Certificate2? GetCert(string domain)
	{
		if (_challengeCerts.TryGetValue(domain, out var cert))
		{
			return cert;
		}

		cert = GetCertAsync(domain).GetAwaiter().GetResult();
		if (cert == null)
		{
			return cert;
		}

		_challengeCerts[domain] = cert;
		return cert;
	}

	public async Task RemoveCertAsync(Guid id)
	{
		var cert = await _certRepository.GetAsync(x => x.Id == id);
		if (cert == null) return;
		await _certRepository.DeleteAsync(cert);
		// todo: remove cert jon.
	}

	private async Task IssueCertAsync(Cert cert)
	{
		CertInfo? certInfo = default;
		var status = AcmeStatus.NotIssued;
		_logger.LogInformation("Running cert issue job.");
		try
		{
			if (cert.DnsProviderId.HasValue)
			{
				cert.DnsProvider = await _dnsProviderRepository.GetAsync(x => x.Id == cert.DnsProviderId.Value);
			}
			var provider = _serviceProvider.GetKeyedService<IAcmeProvider>(cert.Issuer);
			if (provider == null)
			{
				_logger.LogError($"Invalid cert issuer: {cert.Issuer}");
				return;
			}
			certInfo = await provider.CreateCertAsync(cert);
			status = AcmeStatus.Issued;
			_logger.LogInformation($"Cert issued successfully. Domain: {cert.Domain}");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Cert issued failed. Domain: {cert.Domain}");
			status = AcmeStatus.IssueFailed;
		}

		var entity = await _certRepository.GetAsync(x => x.Id == cert.Id);
		if (entity == null) return;
		entity.UpdateCertInfo(certInfo)
			.UpdateStatus(status)
			.UpdateExpires();
		await _certRepository.UpdateAsync(entity);
		if (cert is { DnsProviderId: not null, DnsProvider: not null })
		{
			await _dnsProviderRepository.UpdateAsync(cert.DnsProvider);
		}
		_certJobs.TryRemove(entity.Domain, out _);
	}

	private async Task<X509Certificate2?> GetCertAsync(string domain)
	{
		var certItem = await _certRepository.GetAsync(x => x.Domain == domain);
		if (certItem == null)
		{
			// try to get wildcard cert
			var wildcard = "*" + domain.Substring(domain.IndexOf("."));
			certItem = await _certRepository.GetAsync(
				x => x.Domain == wildcard && x.CertType == CertType.WildcardDomain);
			if (certItem == null) return null;
		}

		_logger.LogInformation($"Select new cert for domain: {domain}, certId: {certItem.Id}");
		var provider = _serviceProvider.GetKeyedService<IAcmeProvider>(certItem.Issuer);
		if (provider != null && certItem.CertInfo != null)
		{
			var pfx = await provider.ReadCertFileAsync(certItem.CertInfo.File);
			var cert = new X509Certificate2(pfx, certItem.CertInfo.Password, X509KeyStorageFlags.Exportable);
			return cert;
		}

		return null;
	}
}