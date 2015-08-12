using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronTuring
{
    static class IEnumerableExtesions
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> sequence, T item)
        {
            return sequence.Concat(new[] { item });
        }
    }
    class Util
    {
    }
}
