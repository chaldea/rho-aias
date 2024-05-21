using Certes;
using Certes.Acme;
using Certes.Acme.Resource;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias.Acme.LetsEncrypt;

internal class LetsEncryptAcmeProvider : IAcmeProvider
{
	private readonly ILogger<LetsEncryptAcmeProvider> _logger;
	private readonly IMemoryCache _memoryCache;
	private readonly IServiceProvider _serviceProvider;
	private readonly IOptions<RhoAiasLetsEncryptOptions> _options;

	public LetsEncryptAcmeProvider(
		ILogger<LetsEncryptAcmeProvider> logger,
		IOptions<RhoAiasLetsEncryptOptions> options,
		IMemoryCache memoryCache,
		IServiceProvider serviceProvider)
	{
		_logger = logger;
		_options = options;
		_memoryCache = memoryCache;
		_serviceProvider = serviceProvider;
	}

	public async Task<CertInfo> CreateCertAsync(Cert cert)
	{
		var context = await CreateContextAsync(cert.Email);
		var order = await context.NewOrder(new[] { cert.Domain });
		var authz = (await order.Authorizations()).First();
		// validate cert ownership
		if (cert.CertType == CertType.WildcardDomain)
			await DnsChallengeAsync(authz, context, cert);
		else
			await HttpChallengeAsync(authz);
		var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);
		var opt = _options.Value;
		var certificateChain = await order.Generate(new CsrInfo
		{
			CountryName = opt.CountryName,
			State = opt.State,
			Locality = opt.Locality,
			Organization = opt.Organization,
			OrganizationUnit = opt.OrganizationUnit,
			CommonName = cert.Domain
		}, privateKey);

		var certPassword = "Aa123456";
		var pfxBuilder = certificateChain.ToPfx(privateKey);
		var pfx = pfxBuilder.Build("my-cert", certPassword);
		var certPath = Utilities.EnsurePath(AppContext.BaseDirectory, opt.CertRootDirectory);
		var certFile = cert.GetFileName();
		var certFullPath = Path.Combine(certPath, certFile);
		if (File.Exists(certFullPath)) File.Delete(certFullPath);
		await File.WriteAllBytesAsync(certFullPath, pfx);
		return new CertInfo
		{
			File = certFile,
			Password = certPassword,
			CountryName = opt.CountryName,
			State = opt.State,
			Locality = opt.Locality,
			Organization = opt.Organization,
			OrganizationUnit = opt.OrganizationUnit,
			CommonName = cert.Domain
		};
	}

	public async Task<byte[]> ReadCertFileAsync(string fileName)
	{
		var path = Utilities.EnsurePath(AppContext.BaseDirectory, _options.Value.CertRootDirectory);
		var certPath = Path.Combine(path, fileName);
		if (File.Exists(certPath)) return await File.ReadAllBytesAsync(certPath);

		return Array.Empty<byte>();
	}

	private async Task<AcmeContext> CreateContextAsync(string email)
	{
		var path = Utilities.EnsurePath(AppContext.BaseDirectory, _options.Value.CertRootDirectory);
		var keyPath = Path.Combine(path, email + ".pem");
		if (File.Exists(keyPath))
		{
			var pemKey = await File.ReadAllTextAsync(keyPath);
			var accountKey = KeyFactory.FromPem(pemKey);
			var context = new AcmeContext(WellKnownServers.LetsEncryptV2, accountKey);
			await context.Account();
			return context;
		}
		else
		{
			_logger.LogInformation($"Create cert key: {keyPath}");
			var context = new AcmeContext(WellKnownServers.LetsEncryptV2);
			await context.NewAccount(email, true);
			var pemKey = context.AccountKey.ToPem();
			await File.WriteAllTextAsync(keyPath, pemKey);
			return context;
		}
	}

	private async Task HttpChallengeAsync(IAuthorizationContext authz)
	{
		var httpChallenge = await authz.Http();
		if (httpChallenge == null) throw new Exception("Invalid httpChallenge.");
		_memoryCache.Set(httpChallenge.Token, httpChallenge.KeyAuthz, TimeSpan.FromMinutes(20));
		await RetryValidateAsync(httpChallenge);
	}

	private async Task DnsChallengeAsync(IAuthorizationContext authz, IAcmeContext acme, Cert cert)
	{
		var dnsChallenge = await authz.Dns();
		if (dnsChallenge == null) throw new Exception("Invalid dnsChallenge.");
		var dnsTxt = acme.AccountKey.DnsTxt(dnsChallenge.Token);
		var dnsProvider = _serviceProvider.GetKeyedService<IDnsProvider>(cert.DnsProvider.Provider);
		var existRecordId = await dnsProvider.ExistsAsync(cert.DnsProvider, cert.TrimDomain());
		if (existRecordId != null)
		{
			// delete _acme-challenge TXT record.
			var result = await dnsProvider.RemoveAsync(cert.DnsProvider, existRecordId);
			if (!result) _logger.LogWarning("Delete dns record failed.");
		}
		var dnsRecordId = await dnsProvider.CreateAsync(cert.DnsProvider, cert.TrimDomain(), dnsTxt);
		if (dnsRecordId == null) throw new Exception("Add dns record failed, Please check the dns provider configuration.");
		await RetryValidateAsync(dnsChallenge);
	}

	private async Task RetryValidateAsync(IChallengeContext context)
	{
		await Task.Delay(5000);
		var challenge = await context.Validate();
		for (var i = 0; i < 50; i++)
		{
			if (challenge.Status == ChallengeStatus.Valid) break;
			if (challenge.Status == ChallengeStatus.Invalid) throw new Exception("Challenge status is Invalid");
			if (i == 49) throw new Exception($"Challenge validate failed, after {i} attempts.");
			await Task.Delay(5000);
			challenge = await context.Resource();
			_logger.LogInformation($"Challenge validate failed, retry {i}...");
		}
	}
}