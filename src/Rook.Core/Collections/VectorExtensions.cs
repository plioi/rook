using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rook.Core.Collections
{
    public static class VectorExtensions
    {
        public static Vector<T> ToVector<T>(this IEnumerable<T> items)
        {
            return new ArrayVector<T>(items.ToArray());
        } 
    }
}
