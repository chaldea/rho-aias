using System.Net.Sockets;
using System.Net;

namespace SocketTester
{
    internal class UdpTesterM : ISocketTester
    {
        public bool Compressed { get; set; }
        public int Frequency { get; set; }
        public int TotalPacks { get; set; }
        public int PackSize { get; set; }
        private int _packageIndex;

        public Task RunServerAsync(int port = 9999)
        {
            return Task.Run(async () =>
            {
                var client = new UdpClient(new IPEndPoint(IPAddress.Any, port));
                while (true)
                {
                    var recv = await RecvAsync(client);
                    if (recv.Buffer.Length <=20)
                    {
                        await SendMultiPackages(client, recv.RemoteEndPoint);
                    }
                }
            });
        }

        public Task RunClientAsync(int port = 8888, string ip = "127.0.0.1")
        {
            return Task.Run(async () =>
            {
                var client = new UdpClient();
                client.Connect(IPAddress.Parse(ip), port);

                // recv from server
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await RecvAsync(client);
                    }
                });
                await SendMultiPackages(client);
                // send EOF
                Console.WriteLine("EOF");
                await SendAsync(client, 10);
            });
        }

        private async Task SendMultiPackages(UdpClient client, IPEndPoint? endPoint = null)
        {
            var list = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                var task = Task.Run(async () =>
                {
                    for (var i = 0; i < 10; i++)
                    {
                        await SendAsync(client, 0, endPoint);
                        await Task.Delay(100);
                    }
                });
                list.Add(task);
            }
            await Task.WhenAll(list);
        }

        private async Task SendAsync(UdpClient client, int len, IPEndPoint? endPoint = null)
        {
            var random = new Random();
            if (len == 0) len = random.Next(100, 8192);
            var data = Util.GenerateRandomBytes(len);
            var checkSum = BitConverter.GetBytes(Util.CheckSum(data));
            if (endPoint != null)
            {
                await client.SendAsync(Combine(data, checkSum), endPoint);
            }
            else
            {
                await client.SendAsync(Combine(data, checkSum));
            }
        }

        private async Task<UdpReceiveResult> RecvAsync(UdpClient client)
        {
            var recv = await client.ReceiveAsync();
            _packageIndex++;
            if (recv.Buffer.Length > 4)
            {
                var checkSum = BitConverter.ToUInt32(recv.Buffer[^4..]);
                var data = recv.Buffer[..^4];
                var sum = Util.CheckSum(data);
                Console.WriteLine($"Recv Package {_packageIndex} Length: {data.Length}, Source: {checkSum}, Dest: {sum}");
            }
            return recv;
        }

        private byte[] Combine(params byte[][] arrays)
        {
            var rv = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
