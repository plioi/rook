using System.Collections.Generic;
using System.Linq;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public static class TypeCheckingExtensions
    {
        public static Vector<T> ToVector<T>(this IEnumerable<T> items)
        {
            return new ArrayVector<T>(items.ToArray());
        }
    }
}