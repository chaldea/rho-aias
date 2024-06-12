using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.Dns.Aliyun;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IRhoAiasConfigurationBuilder AddRhoAiasAliyunDns(this IRhoAiasConfigurationBuilder builder)
    {
        builder.Services.AddRhoAiasAliyunDns();
        return builder;
    }

    public static IServiceCollection AddRhoAiasAliyunDns(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IDnsProvider, AliyunDnsProvider>("Aliyun");
        return services;
    }
}