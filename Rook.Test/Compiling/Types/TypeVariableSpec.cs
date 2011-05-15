using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Rook.Compiling.Types
{
    [TestFixture]
    public sealed class TypeVariableSpec
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

            a.ReplaceTypeVariables(replaceAWithInteger).ShouldBeTheSameAs(NamedType.Integer);
            b.ReplaceTypeVariables(replaceAWithInteger).ShouldBeTheSameAs(b);
        }

        [Test]
        public void ImplementsGetHashCodeAndEqualsBasedOnNameEquality()
        {
            var c = new TypeVariable(2);
            var a2 = new TypeVariable(0);

            a.Equals(a).ShouldBeTrue();
            b.Equals(b).ShouldBeTrue();
            c.Equals(c).ShouldBeTrue();
            a.Equals(a2).ShouldBeTrue();
            a.Equals(b).ShouldBeFalse();
            b.Equals(a).ShouldBeFalse();
            a.Equals(null).ShouldBeFalse();

            (a == a).ShouldBeTrue();
            (b == b).ShouldBeTrue();
            (c == c).ShouldBeTrue();
            (a == a2).ShouldBeTrue();
            (a == b).ShouldBeFalse();
            (b == a).ShouldBeFalse();
            (a == null).ShouldBeFalse();
            (null == a).ShouldBeFalse();

            (a != a).ShouldBeFalse();
            (b != b).ShouldBeFalse();
            (c != c).ShouldBeFalse();
            (a != a2).ShouldBeFalse();
            (a != b).ShouldBeTrue();
            (b != a).ShouldBeTrue();
            (a != null).ShouldBeTrue();
            (null != a).ShouldBeTrue();

            (a.GetHashCode() == a2.GetHashCode()).ShouldBeTrue();
            (a.GetHashCode() == b.GetHashCode()).ShouldBeFalse();
        }
    }
}