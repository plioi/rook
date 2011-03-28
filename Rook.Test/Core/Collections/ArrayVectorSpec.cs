using System.Linq;
using NUnit.Framework;

namespace Rook.Core.Collections
{
    [TestFixture]
    public class ArrayVectorSpec : VectorSpec
    {
        [Test]
        public void ShouldProvideItemCount()
        {
            Vector<int> empty = new ArrayVector<int>();
            Assert.AreEqual(0, empty.Count);

            Vector<int> single = new ArrayVector<int>(0);
            Assert.AreEqual(1, single.Count);
        }

        [Test]
        public void ShouldRemainImmutableEvenWhenSourceArrayIsMutated()
        {
            int[] source = new[] {1, 2};
            Vector<int> vector = new ArrayVector<int>(source);
            
            AssertContents(vector, 1, 2);

            source[0] = -1;
            source[1] = -2;

            AssertContents(vector, 1, 2);
        }

        [Test]
        public void ShouldCreateNewVectorWithNewValueAppended()
        {
            Vector<int> original = new ArrayVector<int>(1, 2, 3);
            AssertContents(original, 1, 2, 3);

            Vector<int> appended = original.Append(4);
            AssertContents(original, 1, 2, 3);
            AssertContents(appended, 1, 2, 3, 4);
        }

        [Test]
        public void ShouldCreateNewVectorWithAlteredCell()
        {
            Vector<int> original = new ArrayVector<int>(1, 2, 3);
            AssertContents(original, 1, 2, 3);
            AssertContents(original.With(0, 10), 10, 2, 3);
            AssertContents(original.With(1, 20), 1, 20, 3);
            AssertContents(original.With(2, 30), 1, 2, 30);
            AssertContents(original, 1, 2, 3);
        }

        [Test]
        public void ShouldGetItemsByIndex()
        {
            Vector<int> vector = new ArrayVector<int>(1, 2, 3);
            Assert.AreEqual(1, vector[0]);
            Assert.AreEqual(2, vector[1]);
            Assert.AreEqual(3, vector[2]);
        }

        [Test]
        public void ShouldCreateSlices()
        {
            Vector<int> slice = new ArrayVector<int>(0, 1, 2, 3, 4, 5, 6).Slice(1, 6);
            AssertContents(slice, 1, 2, 3, 4, 5);
        }

        [Test]
        public void ShouldThrowExceptionWhenGivenIndexIsOutOfRange()
        {
            Vector<int> empty = new ArrayVector<int>();

            AssertIndexOutOfRange(() => empty.With(0, 0));
            AssertIndexOutOfRange(() => empty.With(-1, -1));

            AssertIndexOutOfRange(() => { int value = empty[0]; });
            AssertIndexOutOfRange(() => { int value = empty[-1]; });
        }

        [Test]
        public void ShouldBeEnumerable()
        {
            Vector<int> empty = new ArrayVector<int>();
            Vector<int> nonempty = new ArrayVector<int>(1, 2, 3);

            Assert.AreEqual(new int[] {}, empty.ToArray());
            Assert.AreEqual(new[] {1, 2, 3}, nonempty.ToArray());
        }
    }
}