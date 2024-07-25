using System.Diagnostics;
using System.Net;
using System.Net.Sockets.Kcp.Simple;

namespace SocketTester
{
    internal class KcpTester : ISocketTester
    {
        private readonly Stopwatch _stopwatch = new();
        private long _packIndex = 1;
        public bool Compressed { get; set; } = false;
        public int Frequency { get; set; } = 2000;
        public int TotalPacks { get; set; } = 0;
        public int PackSize { get; set; } = 1024;

        public Task RunServerAsync(int port = 9999)
        {
            return Task.Run(async () =>
            {
                var kcpClient = new SimpleKcpClient(port);
                Task.Run(async () =>
                {
                    while (true)
                    {
                        kcpClient.kcp.Update(DateTimeOffset.UtcNow);
                        await Task.Delay(10);
                    }
                });

                while (TotalPacks == 0 || _packIndex < TotalPacks)
                {
                    try
                    {
                        var readBytes = await RecvAsync(kcpClient);
                        if (readBytes > 0)
                        {
                            await Task.Delay(Frequency);
                            await SendAsync(kcpClient);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                }
            });
        }

        public Task RunClientAsync(int port = 8888, string ip = "127.0.0.1")
        {
            return Task.Run(async () =>
            {
                try
                {
                    var end = new IPEndPoint(IPAddress.Parse(ip), port);
                    var kcpClient = new SimpleKcpClient(port + 1, end);
                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            kcpClient.kcp.Update(DateTimeOffset.UtcNow);
                            await Task.Delay(10);
                        }
                    });
                    await SendAsync(kcpClient);
                    while (TotalPacks == 0 || _packIndex < TotalPacks)
                    {
                        var readBytes = await RecvAsync(kcpClient);
                        if (readBytes > 0)
                        {
                            await Task.Delay(Frequency);
                            await SendAsync(kcpClient);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        private async Task SendAsync(SimpleKcpClient client)
        {
            if (TotalPacks != 0 && _packIndex > TotalPacks)
            {
                return;
            }

            var data = Util.GenerateRandomBytes(PackSize);
            Console.WriteLine($"Send -> PackIndex: {_packIndex} PackSize: {PackSize} Length:{data.Length} CheckSum: {Util.CheckSum(data)}");
            _stopwatch.Restart();
            client.SendAsync(data, data.Length);
        }

        private async Task<int> RecvAsync(SimpleKcpClient client)
        {
            var buffer = await client.ReceiveAsync();
            var readBytes = buffer.Length;
            _stopwatch.Stop();
            if (readBytes > 0)
            {
                Console.WriteLine($"Recv -> PackIndex: {_packIndex} PackSize: {PackSize} Length:{readBytes} CheckSum: {Util.CheckSum(buffer)} Delay: {_stopwatch.ElapsedMilliseconds}ms");
                _packIndex++;
            }
            return readBytes;
        }
    }
}
