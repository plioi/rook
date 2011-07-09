using System;
using System.Collections;
using System.Collections.Generic;

namespace Rook.Core.Collections
{
    public abstract class Vector<T> : IEnumerable<T>
    {
        public abstract int Count { get; }
        public abstract T this[int index] { get; }
        public abstract Vector<T> Append(T value);
        public abstract Vector<T> With(int index, T value);
        public abstract Vector<T> Slice(int startIndexInclusive, int endIndexExclusive);

        protected void DemandValidIndex(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException("Index was outside the bounds of the vector.");
        }

        #region IEnumerable<T>

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}