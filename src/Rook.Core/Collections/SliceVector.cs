using System;

namespace Rook.Core.Collections
{
    public class SliceVector<T> : Vector<T>
    {
        //Invariants:
        //  0 <= startIndexInclusive <= endIndexExclusive <= vector.Count
        //  Also, if vector.Count > 0: startInclusiveIndex < vector.Count

        private readonly Vector<T> vector;
        private readonly int startIndexInclusive;
        private readonly int endIndexExclusive;

        internal SliceVector(Vector<T> vector, int startIndexInclusive, int endIndexExclusive)
        {
            //This constructor is internal to discourage the creation of instances where the underlying
            //Vector is also a SliceVector.  This way, we can ensure that SliceVector honors the 
            //performance characteristics intended for Vector operations.
            //
            //In other words, a slice of a slice of a slice of a vector v should perform lookup as quickly
            //as a slice of that vector v.

            if (endIndexExclusive < startIndexInclusive)
                throw new ArgumentException("endIndexExclusive must be greater than or equal to startIndexInclusive.");

            if (endIndexExclusive > vector.Count)
                throw new ArgumentException("endIndexExclusive must be less than or equal to the source vector's Count.");

            if (startIndexInclusive < 0)
                throw new ArgumentException("startIndexInclusive cannot be negative.");

            if (vector.Count > 0 && startIndexInclusive >= vector.Count)
                throw new ArgumentException("startIndexInclusive must be a valid index for the source vector.");

            this.vector = vector;
            this.startIndexInclusive = startIndexInclusive;
            this.endIndexExclusive = endIndexExclusive;
        }

        public override int Count
        {
            get { return endIndexExclusive - startIndexInclusive; }
        }

        public override T this[int index]
        {
            get
            {
                DemandValidIndex(index);

                return vector[startIndexInclusive + index];
            }
        }

        public override Vector<T> Append(T value)
        {
            bool includesLastItemOfUnderlyingVector = endIndexExclusive == vector.Count;

            Vector<T> alteredUnderlyingVector = includesLastItemOfUnderlyingVector
                                                    ? vector.Append(value)
                                                    : vector.With(endIndexExclusive, value);

            return new SliceVector<T>(alteredUnderlyingVector, startIndexInclusive, endIndexExclusive + 1);
        }

        public override Vector<T> With(int index, T value)
        {
            DemandValidIndex(index);
            return new SliceVector<T>(vector.With(startIndexInclusive + index, value), startIndexInclusive, endIndexExclusive);
        }

        public override Vector<T> Slice(int startIndexInclusive, int endIndexExclusive)
        {
            return new SliceVector<T>(vector,
                                      this.startIndexInclusive + startIndexInclusive,
                                      this.startIndexInclusive + endIndexExclusive);
        }
    }
}