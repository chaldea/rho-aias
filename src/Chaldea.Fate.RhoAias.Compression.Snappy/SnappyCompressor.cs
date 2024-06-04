using System.IO.Compression;
using Snappier;

namespace Chaldea.Fate.RhoAias.Compression.Snappy;

internal class SnappyCompressor : ICompressor
{
    public Stream Compress(Stream networkStream)
    {
        return new SnappyStream(networkStream, CompressionMode.Compress);
    }

    public Stream Decompress(Stream networkStream)
    {
        return new SnappyStream(networkStream, CompressionMode.Decompress);
    }

    public async Task CompressAsync(Stream localStream, Stream serverStream)
    {
        await using var compressor = new SnappyStream(localStream, CompressionMode.Compress);
        await localStream.CopyToAsync(compressor);
    }

    public async Task DecompressAsync(Stream serverStream, Stream localStream)
    {
        await using var compressor = new SnappyStream(serverStream, CompressionMode.Decompress);
        await compressor.CopyToAsync(localStream);
    }
}