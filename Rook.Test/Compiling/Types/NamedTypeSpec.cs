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
            Create("A").Name.ShouldEqual("A");
            Create("B", Create("A")).Name.ShouldEqual("B");
        }

        [Test]
        public void HasZeroOrMoreInnerTypes()
        {
            Create("A").InnerTypes.Count().ShouldEqual(0);

            Create("B", Create("A")).InnerTypes.ShouldList(Create("A"));

            Create("C", Create("B", Create("A"))).InnerTypes.ShouldList(Create("B", Create("A")));
        }

        [Test]
        public void HasAStringRepresentation()
        {
            Create("A").ToString().ShouldEqual("A");
            Create("A", Create("B")).ToString().ShouldEqual("A<B>");
            Create("A", Create("B", Create("C"), Create("D"))).ToString().ShouldEqual("A<B<C, D>>");
        }

        [Test]
        public void IsCreatedLazilyWithMemoization()
        {
            Create("A").ShouldBeTheSameAs(Create("A"));
            Create("B", Create("A")).ShouldBeTheSameAs(Create("B", Create("A")));
        }

        [Test]
        public void CanBeCreatedFromConvenienceFactories()
        {
            NamedType.Dynamic.ShouldBeTheSameAs(Create("dynamic"));
            NamedType.Void.ShouldBeTheSameAs(Create("Rook.Core.Void"));
            NamedType.Integer.ShouldBeTheSameAs(Create("int"));
            NamedType.Boolean.ShouldBeTheSameAs(Create("bool"));
            NamedType.Enumerable(NamedType.Integer).ShouldBeTheSameAs(Create("System.Collections.Generic.IEnumerable", Create("int")));
            NamedType.Vector(NamedType.Integer).ShouldBeTheSameAs(Create("Rook.Core.Collections.Vector", Create("int")));
            NamedType.Nullable(NamedType.Integer).ShouldBeTheSameAs(Create("Rook.Core.Nullable", Create("int")));
            NamedType.Function(NamedType.Integer).ShouldBeTheSameAs(Create("System.Func", Create("int")));
            NamedType.Function(new[] { NamedType.Boolean, NamedType.Enumerable(NamedType.Boolean) }, NamedType.Integer)
                .ShouldBeTheSameAs(Create("System.Func", Create("bool"), Create("System.Collections.Generic.IEnumerable", Create("bool")), Create("int")));
        }

        [Test]
        public void CanDetermineWhetherTheTypeContainsASpecificTypeVariable()
        {
            var x = new TypeVariable(12345);

            Create("A").Contains(x).ShouldBeFalse();
            Create("A", x).Contains(x).ShouldBeTrue();
            Create("A", Create("B", x)).Contains(x).ShouldBeTrue();
        }

        [Test]
        public void CanFindAllDistinctOccurrencesOfContainedTypeVariables()
        {
            var x = new TypeVariable(0);
            var y = new TypeVariable(1);
            var z = new TypeVariable(2);

            Create("A").FindTypeVariables().ShouldBeEmpty();
            Create("A", Create("B")).FindTypeVariables().ShouldBeEmpty();
            Create("A", x, y, z).FindTypeVariables().ShouldList(x, y, z);
            Create("A", Create("B", x, y), Create("C", y, z)).FindTypeVariables().ShouldList(x, y, z);
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
            concrete.ReplaceTypeVariables(replaceAWithInteger).ShouldBeTheSameAs(concrete);
            concrete.ReplaceTypeVariables(replaceBWithA).ShouldBeTheSameAs(concrete);
            concrete.ReplaceTypeVariables(replaceBoth).ShouldBeTheSameAs(concrete);

            Create("A", a, b, a).ReplaceTypeVariables(replaceAWithInteger).ShouldBeTheSameAs(Create("A", NamedType.Integer, b, NamedType.Integer));
            Create("B", b, a, b).ReplaceTypeVariables(replaceAWithInteger).ShouldBeTheSameAs(Create("B", b, NamedType.Integer, b));

            Create("A", a, b, a).ReplaceTypeVariables(replaceBWithA).ShouldBeTheSameAs(Create("A", a, a, a));
            Create("B", b, a, b).ReplaceTypeVariables(replaceBWithA).ShouldBeTheSameAs(Create("B", a, a, a));
            
            //Unlike the type unification/normlization substitutions, these substitutions are ignorant of
            //chains like { b -> a, a -> int }.
            Create("A", a, b, a).ReplaceTypeVariables(replaceBoth).ShouldBeTheSameAs(Create("A", NamedType.Integer, a, NamedType.Integer));
            Create("B", b, a, b).ReplaceTypeVariables(replaceBoth).ShouldBeTheSameAs(Create("B", a, NamedType.Integer, a));
        }

        private static DataType Create(string name, params DataType[] innerTypes)
        {
            return NamedType.Create(name, innerTypes);
        }
    }
}