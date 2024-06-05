using System.Buffers;
using System.Security.Claims;

namespace Chaldea.Fate.RhoAias;

public static class Extensions
{
    public static Guid UserId(this ClaimsPrincipal user)
    {
        var sub = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (sub != null)
        {
            return Guid.Parse(sub.Value);
        }

        return Guid.Empty;
    }

    public static async Task CopyToAsync(this Stream source, Stream destination, bool flush, CancellationToken cancellationToken)
    {
        const int DefaultCopyBufferSize = 81920;
        var bufferSize = DefaultCopyBufferSize;
        if (source.CanSeek)
        {
            var length = source.Length;
            var position = source.Position;
            if (length <= position)
            {
                bufferSize = 1;
            }
            else
            {
                var remaining = length - position;
                if (remaining > 0)
                {
                    bufferSize = (int)Math.Min(bufferSize, remaining);
                }
            }
        }

        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
        if (!destination.CanWrite)
        {
            if (destination.CanRead)
            {
                throw new Exception();
            }

            throw new Exception();
        }

        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
                if (flush) await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}