namespace Chaldea.Fate.RhoAias;

internal static class MemoryExtensions
{
    public static void CopyTo<T>(this T[] source, Memory<T> memory, int index)
    {
        new ReadOnlySpan<T>(source).CopyTo(memory.Span.Slice(index));
    }

    public static void CopyTo<T>(this ReadOnlyMemory<T> source, Memory<T> memory, int index)
    {
        source.Span.CopyTo(memory.Span.Slice(index));
    }

    public static void CopyTo<T>(this ReadOnlyMemory<T> source, T[] buffer, int index)
    {
        Span<T> span = buffer;
        source.Span.CopyTo(span.Slice(index));
    }

    public static ReadOnlyMemory<T> Combine<T>(this T[] source, ReadOnlyMemory<T> memory)
    {
        var data = new T[source.Length + memory.Length];
        source.CopyTo(data, 0);
        memory.CopyTo(data, source.Length);
        return data;
    }
}
