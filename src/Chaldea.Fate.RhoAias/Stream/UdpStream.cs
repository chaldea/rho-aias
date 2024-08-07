using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace Chaldea.Fate.RhoAias;

internal sealed class UdpStream : Stream
{
    private readonly UdpClient _client;
    private readonly Channel<byte[]>? _channel;
    private readonly IPEndPoint? _remoteEndPoint;
    private byte[] _leftBytes = [];

    public UdpStream(UdpClient client, IPEndPoint? remoteEndPoint, Channel<byte[]>? channel)
    {
        _client = client;
        _remoteEndPoint = remoteEndPoint;
        _channel = channel;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            byte[] data;
            if (_channel != null)
            {
                data = await _channel.Reader.ReadAsync(cancellationToken);
            }
            else
            {
                var recv = await _client.ReceiveAsync(cancellationToken);
                data = recv.Buffer;
            }
            var lenBytes = BitConverter.GetBytes(data.Length);
            lenBytes.CopyTo(buffer);
            data.CopyTo(buffer, 4);
            return data.Length + 4;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        ReadOnlyMemory<byte> data;
        if (_leftBytes is { Length: > 0 })
        {
            var mem = new Memory<byte>(new byte[_leftBytes.Length + buffer.Length]);
            _leftBytes.CopyTo(mem, 0);
            buffer.CopyTo(mem, _leftBytes.Length);
            data = mem;
            _leftBytes = Array.Empty<byte>();
        }
        else
        {
            data = buffer;
        }
        if (data.Length > 4)
        {
            var len = BitConverter.ToInt32(data[0..4].Span);
            if (len <= data.Length - 4)
            {
                var offset = len + 4;
                var dataSend = data[4..offset];
                var dataLeft = data[offset..];
                if (_remoteEndPoint != null)
                {
                    await _client.SendAsync(dataSend, _remoteEndPoint, cancellationToken);
                }
                else
                {
                    await _client.SendAsync(dataSend, cancellationToken);
                }

                if (dataLeft.Length > 0)
                {
                    await WriteAsync(dataLeft, cancellationToken);
                }
            }
            else
            {
                _leftBytes = data.ToArray();
            }
        }
        else
        {
            Console.WriteLine("ERROR: data length error.");
        }
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
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
