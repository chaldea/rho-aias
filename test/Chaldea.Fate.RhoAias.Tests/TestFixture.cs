// The Chaldea licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chaldea.Fate.RhoAias.Tests
{
    public class TestFixture : IDisposable
    {
        public IServiceProvider Services { get; private set; }

        public TestFixture()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddLogging();
            services.AddRhoAias(configuration);
            Services = services.BuildServiceProvider();
        }

        public void Dispose()
        {
            if (Services is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
