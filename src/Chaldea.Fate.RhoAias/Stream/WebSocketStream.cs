using Microsoft.AspNetCore.Connections.Features;

namespace Chaldea.Fate.RhoAias;

internal sealed class WebSocketStream : Stream
{
    private readonly Stream readStream;
    private readonly Stream wirteStream;
    private readonly IConnectionLifetimeFeature lifetimeFeature;

    public WebSocketStream(IConnectionLifetimeFeature lifetimeFeature, IConnectionTransportFeature transportFeature, ICompressor? compressor)
    {
        var input = transportFeature.Transport.Input.AsStream();
        var output = transportFeature.Transport.Output.AsStream();
        if (compressor != null)
        {
            this.readStream = compressor.Decompress(input);
            this.wirteStream = compressor.Compress(output);
        }
        else
        {
            this.readStream = input;
            this.wirteStream = output;
        }

        this.lifetimeFeature = lifetimeFeature;
    }

    public WebSocketStream(Stream stream)
    {
        this.readStream = stream;
        this.wirteStream = stream;
        this.lifetimeFeature = null;
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

    public override void Flush()
    {
        this.wirteStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return this.wirteStream.FlushAsync(cancellationToken);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return this.readStream.Read(buffer, offset, count);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return this.readStream.ReadAsync(buffer, cancellationToken);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return this.readStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        this.wirteStream.Write(buffer, offset, count);
        this.wirteStream.Flush();
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        this.wirteStream.Write(buffer);
        this.wirteStream.Flush();
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await this.wirteStream.WriteAsync(buffer, offset, count, cancellationToken);
        await this.wirteStream.FlushAsync(cancellationToken);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await this.wirteStream.WriteAsync(buffer, cancellationToken);
        await this.wirteStream.FlushAsync(cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        this.lifetimeFeature?.Abort();
    }

    public override ValueTask DisposeAsync()
    {
        this.lifetimeFeature?.Abort();
        return ValueTask.CompletedTask;
    }
}