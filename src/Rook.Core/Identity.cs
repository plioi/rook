using System;

namespace Rook.Core
{
    public class Identity<T>
    {
        public Identity(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }

        public void Update(Func<T, T> getNextValue)
        {
            Value = getNextValue(Value);
        }
    }
}