using Chaldea.Fate.RhoAias;
using Chaldea.Fate.RhoAias.Compression.Snappy;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IRhoAiasConfigurationBuilder AddRhoAiasSnappyCompression(this IRhoAiasConfigurationBuilder builder)
        {
            builder.Services.AddRhoAiasSnappyCompression();
            return builder;
        }

        public static IServiceCollection AddRhoAiasSnappyCompression(this IServiceCollection service)
        {
            service.Replace(new ServiceDescriptor(typeof(ICompressor), typeof(SnappyCompressor), ServiceLifetime.Singleton));
            return service;
        }
    }
}