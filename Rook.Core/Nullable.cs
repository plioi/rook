using System;

namespace Rook.Core
{
    public class Nullable<T>
    {
        private readonly T value;

        public Nullable(T value)
        {
            if ((object)value == null)
                throw new ArgumentNullException("value");

            this.value = value;
        }

        public T Value
        {
            get { return value; }
        }
    }
}
