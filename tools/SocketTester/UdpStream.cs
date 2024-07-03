using System.Net;
using System.Net.Sockets;

namespace SocketTester
{
    internal class UdpStream : Stream
    {
        private readonly UdpClient _client;
        private readonly bool _remoteWrite;
        private IPEndPoint? _remoteEndPoint;

        public IPEndPoint RemoteEndPoint => _remoteEndPoint;

        public UdpStream(UdpClient client, bool remoteWrite)
        {
            _client = client;
            _remoteWrite = remoteWrite;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var recv = _client.Receive(ref _remoteEndPoint);
            recv.CopyTo(buffer, 0);
            return recv.Length;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var recv = await _client.ReceiveAsync(cancellationToken);
            _remoteEndPoint = recv.RemoteEndPoint;
            recv.Buffer.CopyTo(buffer, 0);
            return recv.Buffer.Length;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        {
            var recv = await _client.ReceiveAsync(cancellationToken);
            _remoteEndPoint = recv.RemoteEndPoint;
            recv.Buffer.CopyTo(buffer);
            return recv.Buffer.Length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_remoteWrite && _remoteEndPoint != null)
            {
                _client.Send(buffer, count, _remoteEndPoint);
            }
            else
            {
                _client.Send(buffer, count);
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_remoteWrite && _remoteEndPoint != null)
            {
                await _client.SendAsync(buffer, count, _remoteEndPoint);
            }
            else
            {
                await _client.SendAsync(buffer, count);
            }
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        {
            if (_remoteWrite && _remoteEndPoint != null)
            {
                await _client.SendAsync(buffer, _remoteEndPoint, cancellationToken);
            }
            else
            {
                await _client.SendAsync(buffer, cancellationToken);
            }
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }

    internal static class Ext
    {
        public static UdpStream GetStream(this UdpClient client, bool remoteWrite = false)
        {
            return new UdpStream(client, remoteWrite);
        }
    }
}
