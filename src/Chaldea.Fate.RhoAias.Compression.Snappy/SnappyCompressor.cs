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
}