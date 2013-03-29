using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Core.Collections;

namespace Rook.Core
{
    public abstract class Prelude
    {
        protected static T __block__<T>(Func<T> func)
        {
            return func();
        }

        protected static void __evaluate__(object expression)
        {
            
        }

        protected static Vector<T> __vector__<T>(params T[] items)
        {
            return new ArrayVector<T>(items);
        }

        protected static Void Print<T>(T value)
        {
            Console.WriteLine(value);
            return Void.Value;
        }

        protected static Nullable<T> Nullable<T>(T item)
        {
            return new Nullable<T>(item);
        }

        protected static T First<T>(IEnumerable<T> items)
        {
            return items.First();
        }

        protected static IEnumerable<T> Take<T>(IEnumerable<T> items, int count)
        {
            return items.Take(count);
        }

        protected static IEnumerable<T> Skip<T>(IEnumerable<T> items, int count)
        {
            return items.Skip(count);
        }

        protected static bool Any<T>(IEnumerable<T> items)
        {
            return items.Any();
        }

        protected static int Count<T>(IEnumerable<T> items)
        {
            return items.Count();
        }

        protected static IEnumerable<TOutput> Select<TInput, TOutput>(IEnumerable<TInput> items, Func<TInput, TOutput> selector)
        {
            return items.Select(selector);
        }

        protected static IEnumerable<T> Where<T>(IEnumerable<T> items, Func<T, bool> predicate)
        {
            return items.Where(predicate);
        }

        protected static IEnumerable<T> Each<T>(Vector<T> vector)
        {
            return vector;
        }

        protected static T __index__<T>(Vector<T> vector, int index)
        {
            return vector[index];
        }

        protected static Vector<T> __slice__<T>(Vector<T> vector, int startIndexInclusive, int endIndexExclusive)
        {
            return vector.Slice(startIndexInclusive, endIndexExclusive);
        }

        protected static Vector<T> Append<T>(Vector<T> vector, T value)
        {
            return vector.Append(value);
        }

        protected static Vector<T> With<T>(Vector<T> vector, int index, T value)
        {
            return vector.With(index, value);
        }
    }
}