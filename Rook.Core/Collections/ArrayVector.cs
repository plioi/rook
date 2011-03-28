using System;

namespace Rook.Core.Collections
{
    public class ArrayVector<T> : Vector<T>
    {
        private readonly T[] items;

        public ArrayVector(params T[] items)
        {
            this.items = Clone(items);
        }

        public override int Count
        {
            get { return items.Length; }
        }

        public override T this[int index]
        {
            get
            {
                DemandValidIndex(index);

                return items[index];
            }
        }

        public override Vector<T> Append(T value)
        {
            T[] array = new T[Count + 1];
            Array.Copy(items, array, Count);
            array[Count] = value;
            return new ArrayVector<T>(array);
        }

        public override Vector<T> With(int index, T value)
        {
            DemandValidIndex(index);

            T[] array = Clone(items);
            array[index] = value;
            return new ArrayVector<T>(array);
        }

        public override Vector<T> Slice(int startIndexInclusive, int endIndexExclusive)
        {
            return new SliceVector<T>(this, startIndexInclusive, endIndexExclusive);
        }

        private static T[] Clone(T[] source)
        {
            T[] destination = new T[source.Length];
            Array.Copy(source, destination, source.Length);
            return destination;
        }
    }
}