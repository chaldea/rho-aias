using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.Dns.Aliyun;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IRhoAiasConfigurationBuilder AddAhoAiasAliyunDns(this IRhoAiasConfigurationBuilder builder)
	{
		builder.Services.AddKeyedSingleton<IDnsProvider, AliyunDnsProvider>("Aliyun");
		return builder;
	}
}