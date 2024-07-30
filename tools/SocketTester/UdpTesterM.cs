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
                    await RecvAsync(client);
                }
            });
        }

        public Task RunClientAsync(int port = 8888, string ip = "127.0.0.1")
        {
            return Task.Run(() =>
            {
                var client = new UdpClient();
                client.Connect(IPAddress.Parse(ip), port);

                for (var i = 0; i < 10; i++)
                {
                    Task.Run(async () =>
                    {
                        for (var i = 0; i < 10; i++)
                        {
                            await SendAsync(client);
                            await Task.Delay(100);
                        }
                    });
                }
            });
        }

        private async Task SendAsync(UdpClient client)
        {
            var random = new Random();
            var packSize = random.Next(10, 10000);
            var data = Util.GenerateRandomBytes(packSize);
            var checkSum = BitConverter.GetBytes(Util.CheckSum(data));
            await client.SendAsync(Combine(data, checkSum));
        }

        private async Task<int> RecvAsync(UdpClient client)
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
            return recv.Buffer.Length;
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
