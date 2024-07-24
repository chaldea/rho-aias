using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using Snappier;
using System.IO.Compression;

namespace SocketTester
{
    internal class UdpTester : ISocketTester
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
                var client = new UdpClient(new IPEndPoint(IPAddress.Any, port));
                var stream = client.GetStream(true);
                await using var sendStream = GetStream(stream, Compressed, CompressionMode.Compress);
                await using var recvStream = GetStream(stream, Compressed, CompressionMode.Decompress);
                while (TotalPacks == 0 || _packIndex < TotalPacks)
                {
                    try
                    {
                        var readBytes = await RecvAsync(recvStream);
                        if (readBytes > 0)
                        {
                            await Task.Delay(Frequency);
                            await SendAsync(sendStream);
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
                    var client = new UdpClient();
                    client.Connect(IPAddress.Parse(ip), port);
                    var stream = client.GetStream();
                    await using var sendStream = GetStream(stream, Compressed, CompressionMode.Compress);
                    await using var recvStream = GetStream(stream, Compressed, CompressionMode.Decompress);
                    await SendAsync(sendStream);
                    while (TotalPacks == 0 || _packIndex < TotalPacks)
                    {
                        var readBytes = await RecvAsync(recvStream);
                        if (readBytes > 0)
                        {
                            await Task.Delay(Frequency);
                            await SendAsync(sendStream);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        private Stream GetStream(UdpStream stream, bool compressed, CompressionMode mode)
        {
            if (compressed)
            {
                return new SnappyStream(stream, mode);
            }

            return stream;
        }

        private async Task SendAsync(Stream stream)
        {
            if (TotalPacks != 0 && _packIndex > TotalPacks)
            {
                return;
            }
            var data = Util.GenerateRandomBytes(PackSize);
            Console.WriteLine($"Send -> PackIndex: {_packIndex} PackSize: {PackSize} Length:{data.Length} CheckSum: {Util.CheckSum(data)}");
            _stopwatch.Restart();
            await stream.WriteAsync(data);
            await stream.FlushAsync();
        }

        private async Task<int> RecvAsync(Stream stream)
        {
            var buffer = new byte[1024];
            var readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            _stopwatch.Stop();
            if (readBytes > 0)
            {
                Console.WriteLine($"Recv -> PackIndex: {_packIndex} PackSize: {PackSize} Length:{readBytes} CheckSum: {Util.CheckSum(buffer[..readBytes])} Delay: {_stopwatch.ElapsedMilliseconds}ms");
                _packIndex++;
            }
            return readBytes;
        }
    }
}
