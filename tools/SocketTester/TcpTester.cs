using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using Snappier;

namespace SocketTester;

internal class TcpTester : ISocketTester
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
            var listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            listener.Start();
            var client = await listener.AcceptTcpClientAsync();
            await using var sendStream = GetStream(client, Compressed, CompressionMode.Compress);
            await using var recvStream = GetStream(client, Compressed, CompressionMode.Decompress);
            while (TotalPacks == 0 || _packIndex < TotalPacks)
            {
                var readBytes = await RecvAsync(recvStream);
                if (readBytes > 0)
                {
                    await Task.Delay(Frequency);
                    await SendAsync(sendStream);
                }
            }
        });
    }

    public Task RunClientAsync(int port = 8888, string ip = "127.0.0.1")
    {
        return Task.Run(async () =>
        {
            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Parse(ip), port);
            await using var sendStream = GetStream(client, Compressed, CompressionMode.Compress);
            await using var recvStream = GetStream(client, Compressed, CompressionMode.Decompress);
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
        });
    }

    private Stream GetStream(TcpClient client, bool compressed, CompressionMode mode)
    {
        var stream = client.GetStream();
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
        var buffer = new byte[2048];
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
