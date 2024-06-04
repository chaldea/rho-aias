using System.IO.Compression;

namespace Chaldea.Fate.RhoAias;

public interface ICompressor
{
    Stream Compress(Stream networkStream);
    Stream Decompress(Stream networkStream);
    Task CompressAsync(Stream localStream, Stream serverStream);
    Task DecompressAsync(Stream serverStream, Stream localStream);
}

internal class GZipCompressor : ICompressor
{
    public Stream Compress(Stream networkStream)
    {
        return new GZipStream(networkStream, CompressionMode.Compress);
    }

    public Stream Decompress(Stream networkStream)
    {
       return new GZipStream(networkStream, CompressionMode.Decompress);
    }

    public async Task CompressAsync(Stream localStream, Stream serverStream)
    {
        await using var compressor = new GZipStream(localStream, CompressionMode.Compress);
        await localStream.CopyToAsync(compressor);
    }

    public async Task DecompressAsync(Stream serverStream, Stream localStream)
    {
        await using var compressor = new GZipStream(serverStream, CompressionMode.Decompress);
        await compressor.CopyToAsync(localStream);
    }
}