using System.IO.Compression;

namespace Chaldea.Fate.RhoAias;

public interface ICompressor
{
    Stream Compress(Stream networkStream);
    Stream Decompress(Stream networkStream);
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
}