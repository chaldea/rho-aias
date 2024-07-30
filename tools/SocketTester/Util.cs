using System.IO.Hashing;

namespace SocketTester;

internal static class Util
{
    public static byte[] GenerateRandomBytes(int size = 1024)
    {
        var data = new byte[size];
        var rand = new Random();
        rand.NextBytes(data);
        return data;
    }

    public static uint CheckSum(byte[] data)
    {
        var crc = new Crc32();
        crc.Append(data);
        return crc.GetCurrentHashAsUInt32();
    }
}
