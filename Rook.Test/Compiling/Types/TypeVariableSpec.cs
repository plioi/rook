using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Rook.Compiling.Types
{
    [TestFixture]
    public class TypeVariableSpec
    {
        private TypeVariable a, b;

        [SetUp]
        public void SetUp()
        {
            a = new TypeVariable(0);
            b = new TypeVariable(1);
        }

        [Test]
        public void HasAName()
        {
            a.Name.ShouldEqual("0");
            b.Name.ShouldEqual("1");
        }

        [Test]
        public void HasZeroInnerTypes()
        {
            a.InnerTypes.Count().ShouldEqual(0);
            b.InnerTypes.Count().ShouldEqual(0);
        }

        [Test]
        public void ContainsOnlyItself()
        {
            a.Contains(a).ShouldBeTrue();
            b.Contains(b).ShouldBeTrue();

            a.Contains(b).ShouldBeFalse();
            b.Contains(a).ShouldBeFalse();
        }

        [Test]
        public void YieldsOnlyItselfWhenAskedToFindTypeVariableOccurrences()
        {
            a.FindTypeVariables().ShouldList(a);
            b.FindTypeVariables().ShouldList(b);
        }

        [Test]
        public void CanPerformTypeVariableSubstitutionOnItself()
        {
            IDictionary<TypeVariable, DataType> replaceAWithInteger =
                new Dictionary<TypeVariable, DataType> { { a, NamedType.Integer } };

            a.ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(NamedType.Integer);
            b.ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(b);
        }

        [Test]
        public void HasValueEqualitySemantics()
        {
            a.ShouldEqual(a);
            a.ShouldEqual(new TypeVariable(0));
            a.ShouldNotEqual(b);
            a.ShouldNotEqual(NamedType.Create("A"));

            a.GetHashCode().ShouldEqual(new TypeVariable(0).GetHashCode());
            a.GetHashCode().ShouldNotEqual(b.GetHashCode());
        }
    }
}