using System.Collections.Generic;
using System.Linq;
using Should;
using Xunit;

namespace Rook.Compiling.Types
{
    public class TypeVariableTests
    {
        private TypeVariable a, b;

        public TypeVariableTests()
        {
            a = new TypeVariable(0);
            b = new TypeVariable(1);
        }

        [Fact]
        public void HasAName()
        {
            a.Name.ShouldEqual("0");
            b.Name.ShouldEqual("1");
        }

        [Fact]
        public void HasZeroInnerTypes()
        {
            a.InnerTypes.Count().ShouldEqual(0);
            b.InnerTypes.Count().ShouldEqual(0);
        }

        [Fact]
        public void ContainsOnlyItself()
        {
            a.Contains(a).ShouldBeTrue();
            b.Contains(b).ShouldBeTrue();

            a.Contains(b).ShouldBeFalse();
            b.Contains(a).ShouldBeFalse();
        }

        [Fact]
        public void YieldsOnlyItselfWhenAskedToFindTypeVariableOccurrences()
        {
            a.FindTypeVariables().ShouldList(a);
            b.FindTypeVariables().ShouldList(b);
        }

        [Fact]
        public void CanPerformTypeVariableSubstitutionOnItself()
        {
            IDictionary<TypeVariable, DataType> replaceAWithInteger =
                new Dictionary<TypeVariable, DataType> { { a, NamedType.Integer } };

            a.ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(NamedType.Integer);
            b.ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(b);
        }

        [Fact]
        public void HasValueEqualitySemantics()
        {
            a.ShouldEqual(a);
            a.ShouldEqual(new TypeVariable(0));
            a.ShouldNotEqual(b);
            a.ShouldNotEqual((DataType)new NamedType("A"));

            a.GetHashCode().ShouldEqual(new TypeVariable(0).GetHashCode());
            a.GetHashCode().ShouldNotEqual(b.GetHashCode());
        }
    }
}