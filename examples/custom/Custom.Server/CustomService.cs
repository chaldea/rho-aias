using Chaldea.Fate.RhoAias;

namespace Custom.Server
{
    public class CustomService : IHostedService
    {
        private readonly IProxyManager _proxyManager;
        private readonly IClientManager _clientManager;

        public CustomService(IProxyManager proxyManager, IClientManager clientManager)
        {
            _proxyManager = proxyManager;
            _clientManager = clientManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task InitAsync()
        {
            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = "testing client",
                Token = "1234567890"
            };

            await _clientManager.CreateClientAsync(client);

            var proxy = new Proxy
            {

            };
        }
    }
}
