using System;

namespace Rook.Core.Collections
{
    public abstract class VectorSpec
    {
        protected static void AssertIndexOutOfRange(Action shouldThrow)
        {
            bool threw = false;

            try
            {
                shouldThrow();
            }
            catch (IndexOutOfRangeException ex)
            {
                ex.Message.ShouldEqual("Index was outside the bounds of the vector.");
                threw = true;
            }

            threw.ShouldBeTrue("Expected IndexOutOfRangeException.");
        }
    }
}