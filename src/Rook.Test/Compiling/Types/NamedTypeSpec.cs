using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Rook.Compiling.Types
{
    [TestFixture]
    public class NamedTypeSpec
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
        public void HasValueEqualitySemantics()
        {
            var type = Create("B", Create("A"));
            type.ShouldEqual(type);
            type.ShouldEqual(Create("B", Create("A")));
            type.ShouldNotEqual(Create("B"));
            type.ShouldNotEqual(new TypeVariable(0));

            type.GetHashCode().ShouldEqual(Create("B", Create("A")).GetHashCode());
            type.GetHashCode().ShouldNotEqual(Create("B").GetHashCode());
        }

        [Test]
        public void CanBeCreatedFromConvenienceFactories()
        {
            NamedType.Dynamic.ShouldEqual(Create("dynamic"));
            NamedType.Void.ShouldEqual(Create("Rook.Core.Void"));
            NamedType.Integer.ShouldEqual(Create("int"));
            NamedType.Boolean.ShouldEqual(Create("bool"));
            NamedType.Enumerable(NamedType.Integer).ShouldEqual(Create("System.Collections.Generic.IEnumerable", Create("int")));
            NamedType.Vector(NamedType.Integer).ShouldEqual(Create("Rook.Core.Collections.Vector", Create("int")));
            NamedType.Nullable(NamedType.Integer).ShouldEqual(Create("Rook.Core.Nullable", Create("int")));
            NamedType.Function(NamedType.Integer).ShouldEqual(Create("System.Func", Create("int")));
            NamedType.Function(new[] { NamedType.Boolean, NamedType.Enumerable(NamedType.Boolean) }, NamedType.Integer)
                .ShouldEqual(Create("System.Func", Create("bool"), Create("System.Collections.Generic.IEnumerable", Create("bool")), Create("int")));
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
            concrete.ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(concrete);
            concrete.ReplaceTypeVariables(replaceBWithA).ShouldEqual(concrete);
            concrete.ReplaceTypeVariables(replaceBoth).ShouldEqual(concrete);

            Create("A", a, b, a).ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(Create("A", NamedType.Integer, b, NamedType.Integer));
            Create("B", b, a, b).ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(Create("B", b, NamedType.Integer, b));

            Create("A", a, b, a).ReplaceTypeVariables(replaceBWithA).ShouldEqual(Create("A", a, a, a));
            Create("B", b, a, b).ReplaceTypeVariables(replaceBWithA).ShouldEqual(Create("B", a, a, a));
            
            //Unlike the type unification/normlization substitutions, these substitutions are ignorant of
            //chains like { b -> a, a -> int }.
            Create("A", a, b, a).ReplaceTypeVariables(replaceBoth).ShouldEqual(Create("A", NamedType.Integer, a, NamedType.Integer));
            Create("B", b, a, b).ReplaceTypeVariables(replaceBoth).ShouldEqual(Create("B", a, NamedType.Integer, a));
        }

        private static DataType Create(string name, params DataType[] innerTypes)
        {
            return new NamedType(name, innerTypes);
        }
    }
}