using System;
using System.Linq;
using NUnit.Framework;

namespace Rook.Core.Collections
{
    public abstract class VectorSpec
    {
        protected static void AssertContents<T>(Vector<T> actual, params T[] expected)
        {
            Assert.AreEqual(expected.Length, actual.Count);
            Assert.AreEqual(expected, actual.ToArray());
        }

        protected static void AssertEmpty<T>(Vector<T> actual)
        {
            AssertContents(actual, new T[] {});
        }

        protected static void AssertIndexOutOfRange(Action shouldThrow)
        {
            bool threw = false;

            try
            {
                shouldThrow();
            }
            catch (IndexOutOfRangeException ex)
            {
                Assert.AreEqual("Index was outside the bounds of the vector.", ex.Message);
                threw = true;
            }

            Assert.IsTrue(threw, "Expected IndexOutOfRangeException.");
        }
    }
}