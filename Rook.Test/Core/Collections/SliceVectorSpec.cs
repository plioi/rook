using System;
using System.Linq;
using NUnit.Framework;

namespace Rook.Core.Collections
{
    [TestFixture]
    public class SliceVectorSpec : VectorSpec
    {
        [Test]
        public void ShouldProvideItemCount()
        {
            Assert.AreEqual(0, SliceDigits(0, 0).Count);
            Assert.AreEqual(5, SliceDigits(0, 5).Count);
            Assert.AreEqual(3, SliceDigits(7, 10).Count);
        }

        [Test]
        public void ShouldBeEnumerable()
        {
            Vector<int> empty = SliceDigits(0, 0);
            Vector<int> nonempty = SliceDigits(0, 10);

            Assert.AreEqual(new int[] { }, empty.ToArray());
            Assert.AreEqual(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, nonempty.ToArray());
        }

        [Test]
        public void ShouldSliceFromStartIndexInclusiveToEndIndexExclusive()
        {
            AssertEmpty(SliceDigits(0, 0));
            AssertEmpty(SliceDigits(5, 5));
            AssertEmpty(SliceDigits(9, 9));
            
            AssertContents(SliceDigits(0, 1), 0);
            AssertContents(SliceDigits(0, 2), 0, 1);
            AssertContents(SliceDigits(0, 9), 0, 1, 2, 3, 4, 5, 6, 7, 8);
            AssertContents(SliceDigits(0, 10), 0, 1, 2, 3, 4, 5, 6, 7, 8, 9);

            AssertContents(SliceDigits(5, 6), 5);
            AssertContents(SliceDigits(5, 7), 5, 6);
            AssertContents(SliceDigits(5, 9), 5, 6, 7, 8);
            AssertContents(SliceDigits(5, 10), 5, 6, 7, 8, 9);
        }

        [Test]
        public void ShouldGetItemsByIndex()
        {
            Vector<int> start = SliceDigits(0, 2);
            Assert.AreEqual(0, start[0]);
            Assert.AreEqual(1, start[1]);

            Vector<int> end = SliceDigits(5, 10);
            Assert.AreEqual(5, end[0]);
            Assert.AreEqual(6, end[1]);
            Assert.AreEqual(7, end[2]);
            Assert.AreEqual(8, end[3]);
            Assert.AreEqual(9, end[4]);
        }

        [Test]
        public void ShouldCreateNewVectorWithNewValueAppended()
        {
            Vector<int> interiorSlice = SliceDigits(1, 4);
            Vector<int> tailSlice = SliceDigits(7, 10);

            AssertContents(interiorSlice, 1, 2, 3);
            AssertContents(tailSlice, 7, 8, 9);

            AssertContents(interiorSlice.Append(-1), 1, 2, 3, -1);
            AssertContents(tailSlice.Append(-1), 7, 8, 9, -1);

            AssertContents(interiorSlice, 1, 2, 3);
            AssertContents(tailSlice, 7, 8, 9);
        }

        [Test]
        public void ShouldCreateNewVectorWithAlteredCell()
        {
            Vector<int> interiorSlice = SliceDigits(1, 4);
            Vector<int> tailSlice = SliceDigits(7, 10);

            AssertContents(interiorSlice, 1, 2, 3);
            AssertContents(tailSlice, 7, 8, 9);

            AssertContents(interiorSlice.With(0, 0), 0, 2, 3);
            AssertContents(interiorSlice.With(1, 0), 1, 0, 3);
            AssertContents(interiorSlice.With(2, 0), 1, 2, 0);

            AssertContents(tailSlice.With(0, 0), 0, 8, 9);
            AssertContents(tailSlice.With(1, 0), 7, 0, 9);
            AssertContents(tailSlice.With(2, 0), 7, 8, 0);

            AssertContents(interiorSlice, 1, 2, 3);
            AssertContents(tailSlice, 7, 8, 9);
        }

        [Test]
        public void ShouldCreateSlices()
        {
            Vector<int> sliceOfInteriorSlice = SliceDigits(2, 9).Slice(1, 4);
            Vector<int> sliceOfTailSlice = SliceDigits(3, 10).Slice(1, 4);

            AssertContents(sliceOfInteriorSlice, 3, 4, 5);
            AssertContents(sliceOfTailSlice, 4, 5, 6);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "endIndexExclusive must be greater than or equal to startIndexInclusive.")]
        public void ShouldDemandUpperLimitCannotBeSmallerThanLowerLimit()
        {
            SliceDigits(1, 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "endIndexExclusive must be less than or equal to the source vector's Count.")]
        public void ShouldDemandUpperLimitCannotExceedSourceVectorCount()
        {
            SliceDigits(0, 11);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "startIndexInclusive cannot be negative.")]
        public void ShouldDemandLowerLimitCannotBeNegative()
        {
            SliceDigits(-1, 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "startIndexInclusive must be a valid index for the source vector.")]
        public void ShouldDemandLowerLimitMustBeValidForSoureVector()
        {
            SliceDigits(10, 10);
        }

        [Test]
        public void ShouldAllowTakingAnEmptySliceOfAnEmptySourceVector()
        {
            //This is a test of the internal SliceVector constructor,
            //indirectly since we cannot construct it directly.

            Vector<int> empty = new ArrayVector<int>();

            AssertEmpty(empty.Slice(0, 0));
        }

        [Test]
        public void ShouldThrowExceptionWhenGivenIndexIsOutOfRange()
        {
            Vector<int> fiveToNine = SliceDigits(5, 10);

            AssertIndexOutOfRange(() => fiveToNine.With(5, 5));
            AssertIndexOutOfRange(() => fiveToNine.With(-1, -1));

            AssertIndexOutOfRange(() => { int value = fiveToNine[5]; });
            AssertIndexOutOfRange(() => { int value = fiveToNine[-1]; });
        }

        private static Vector<int> SliceDigits(int startIndexInclusive, int endIndexExclusive)
        {
            ArrayVector<int> digits = new ArrayVector<int>(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            return digits.Slice(startIndexInclusive, endIndexExclusive);
        }
    }
}