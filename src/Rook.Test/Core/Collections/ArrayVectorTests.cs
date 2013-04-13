using System;
using Should;

namespace Rook.Core.Collections
{
    public class ArrayVectorTests
    {
        public void ShouldProvideItemCount()
        {
            Vector<int> empty = new ArrayVector<int>();
            empty.Count.ShouldEqual(0);

            Vector<int> single = new ArrayVector<int>(0);
            single.Count.ShouldEqual(1);
        }

        public void ShouldRemainImmutableEvenWhenSourceArrayIsMutated()
        {
            var source = new[] {1, 2};
            Vector<int> vector = new ArrayVector<int>(source);
            
            vector.ShouldList(1, 2);

            source[0] = -1;
            source[1] = -2;

            vector.ShouldList(1, 2);
        }

        public void ShouldCreateNewVectorWithNewValueAppended()
        {
            Vector<int> original = new ArrayVector<int>(1, 2, 3);
            original.ShouldList(1, 2, 3);

            Vector<int> appended = original.Append(4);
            original.ShouldList(1, 2, 3);
            appended.ShouldList(1, 2, 3, 4);
        }

        public void ShouldCreateNewVectorWithAlteredCell()
        {
            Vector<int> original = new ArrayVector<int>(1, 2, 3);
            original.ShouldList(1, 2, 3);
            original.With(0, 10).ShouldList(10, 2, 3);
            original.With(1, 20).ShouldList(1, 20, 3);
            original.With(2, 30).ShouldList(1, 2, 30);
            original.ShouldList(1, 2, 3);
        }

        public void ShouldGetItemsByIndex()
        {
            Vector<int> vector = new ArrayVector<int>(1, 2, 3);
            vector[0].ShouldEqual(1);
            vector[1].ShouldEqual(2);
            vector[2].ShouldEqual(3);
        }

        public void ShouldCreateSlices()
        {
            Vector<int> slice = new ArrayVector<int>(0, 1, 2, 3, 4, 5, 6).Slice(1, 6);
            slice.ShouldList(1, 2, 3, 4, 5);
        }

        public void ShouldThrowExceptionWhenGivenIndexIsOutOfRange()
        {
            Vector<int> empty = new ArrayVector<int>();

            var actions = new Action[]
            {
                () => empty.With(0, 0),
                () => empty.With(-1, -1),
                () => { int value = empty[0]; },
                () => { int value = empty[-1]; }
            };

            foreach (var action in actions)
                action.ShouldThrow<IndexOutOfRangeException>("Index was outside the bounds of the vector.");
        }

        public void ShouldBeEnumerable()
        {
            Vector<int> empty = new ArrayVector<int>();
            Vector<int> nonempty = new ArrayVector<int>(1, 2, 3);

            empty.ShouldBeEmpty();
            nonempty.ShouldList(1, 2, 3);
        }
    }
}