using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Rook.Compiling.Types
{
    [TestFixture]
    public sealed class NamedTypeSpec
    {
        [Test]
        public void HasAName()
        {
            Assert.AreEqual("A", Create("A").Name);
            Assert.AreEqual("B", Create("B", Create("A")).Name);
        }

        [Test]
        public void HasZeroOrMoreInnerTypes()
        {
            Assert.AreEqual(0, Create("A").InnerTypes.Count());

            Assert.AreEqual(new[] {Create("A")},
                            Create("B", Create("A")).InnerTypes.ToArray());

            Assert.AreEqual(new[] {Create("B", Create("A"))},
                            Create("C", Create("B", Create("A"))).InnerTypes.ToArray());
        }

        [Test]
        public void HasAStringRepresentation()
        {
            Assert.AreEqual("A", Create("A").ToString());
            Assert.AreEqual("A<B>", Create("A", Create("B")).ToString());
            Assert.AreEqual("A<B<C, D>>", Create("A", Create("B", Create("C"), Create("D"))).ToString());
        }

        [Test]
        public void IsCreatedLazilyWithMemoization()
        {
            Assert.AreSame(Create("A"), Create("A"));
            Assert.AreSame(Create("B", Create("A")), Create("B", Create("A")));
        }

        [Test]
        public void CanBeCreatedFromConvenienceFactories()
        {
            Assert.AreSame(Create("dynamic"), NamedType.Dynamic);
            Assert.AreSame(Create("Rook.Core.Void"), NamedType.Void);
            Assert.AreSame(Create("int"), NamedType.Integer);
            Assert.AreSame(Create("bool"), NamedType.Boolean);
            Assert.AreSame(Create("System.Collections.Generic.IEnumerable", Create("int")), NamedType.Enumerable(NamedType.Integer));
            Assert.AreSame(Create("Rook.Core.Collections.Vector", Create("int")), NamedType.Vector(NamedType.Integer));
            Assert.AreSame(Create("Rook.Core.Nullable", Create("int")), NamedType.Nullable(NamedType.Integer));
            Assert.AreSame(Create("System.Func", Create("int")), NamedType.Function(NamedType.Integer));
            Assert.AreSame(Create("System.Func", Create("bool"), Create("System.Collections.Generic.IEnumerable", Create("bool")), Create("int")),
                           NamedType.Function(new[] { NamedType.Boolean, NamedType.Enumerable(NamedType.Boolean) }, NamedType.Integer));
        }

        [Test]
        public void CanDetermineWhetherTheTypeContainsASpecificTypeVariable()
        {
            var x = new TypeVariable(12345);

            Assert.IsFalse(Create("A").Contains(x));
            Assert.IsTrue(Create("A", x).Contains(x));
            Assert.IsTrue(Create("A", Create("B", x)).Contains(x));
        }

        [Test]
        public void CanFindAllDistinctOccurrencesOfContainedTypeVariables()
        {
            var x = new TypeVariable(0);
            var y = new TypeVariable(1);
            var z = new TypeVariable(2);

            Assert.AreEqual(new TypeVariable[] {}, Create("A").FindTypeVariables().ToArray());
            Assert.AreEqual(new TypeVariable[] {}, Create("A", Create("B")).FindTypeVariables().ToArray());
            Assert.AreEqual(new[] { x, y, z },  Create("A", x, y, z).FindTypeVariables().ToArray());
            Assert.AreEqual(new[] { x, y, z },  Create("A", Create("B", x, y), Create("C", y, z)).FindTypeVariables().ToArray());
        }

        [Test]
        public void CanPerformTypeVariableSubstitutios()
        {
            var a = new TypeVariable(0);
            var b = new TypeVariable(1);

            IDictionary<TypeVariable, DataType> replaceAWithInteger =
                new Dictionary<TypeVariable, DataType> { { a, NamedType.Integer } };

            IDictionary<TypeVariable, DataType> replaceBWithA =
                new Dictionary<TypeVariable, DataType> { { b, a } };

            IDictionary<TypeVariable, DataType> replaceBoth =
                new Dictionary<TypeVariable, DataType> { { a, NamedType.Integer }, { b, a } };

            DataType concrete = Create("A", Create("B"));
            Assert.AreSame(concrete, concrete.ReplaceTypeVariables(replaceAWithInteger));
            Assert.AreSame(concrete, concrete.ReplaceTypeVariables(replaceBWithA));
            Assert.AreSame(concrete, concrete.ReplaceTypeVariables(replaceBoth));

            Assert.AreSame(Create("A", NamedType.Integer, b, NamedType.Integer), Create("A", a, b, a).ReplaceTypeVariables(replaceAWithInteger));
            Assert.AreSame(Create("B", b, NamedType.Integer, b), Create("B", b, a, b).ReplaceTypeVariables(replaceAWithInteger));

            Assert.AreSame(Create("A", a, a, a), Create("A", a, b, a).ReplaceTypeVariables(replaceBWithA));
            Assert.AreSame(Create("B", a, a, a), Create("B", b, a, b).ReplaceTypeVariables(replaceBWithA));
            
            //Unlike the type unification/normlization substitutions, these substitutions are ignorant of
            //chains like { b -> a, a -> int }.
            Assert.AreSame(Create("A", NamedType.Integer, a, NamedType.Integer), Create("A", a, b, a).ReplaceTypeVariables(replaceBoth));
            Assert.AreSame(Create("B", a, NamedType.Integer, a), Create("B", b, a, b).ReplaceTypeVariables(replaceBoth));
        }

        private static DataType Create(string name, params DataType[] innerTypes)
        {
            return NamedType.Create(name, innerTypes);
        }
    }
}