using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chaldea.Fate.RhoAias
{
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
    }
}
