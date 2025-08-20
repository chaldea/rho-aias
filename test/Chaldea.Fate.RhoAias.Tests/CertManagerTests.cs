// The Chaldea licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.DependencyInjection;

namespace Chaldea.Fate.RhoAias.Tests
{
    public class CertManagerTests : IClassFixture<TestFixture>
    {
        private readonly ICertManager _certManager;

        public CertManagerTests(TestFixture fixture)
        {
            _certManager = fixture.Services.GetRequiredService<ICertManager>();
        }

        [Fact]
        public async Task CreateCert()
        {
            await _certManager.CreateCertAsync(new Cert
            {
                Domain = "a.sample.com",
                Issuer = "SelfSigned"
            });
        }
    }
}
