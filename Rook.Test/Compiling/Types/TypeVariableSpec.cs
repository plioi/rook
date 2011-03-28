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
            Assert.AreEqual("0", a.Name);
            Assert.AreEqual("1", b.Name);
        }

        [Test]
        public void HasZeroInnerTypes()
        {
            Assert.AreEqual(0, a.InnerTypes.Count());
            Assert.AreEqual(0, b.InnerTypes.Count());
        }

        [Test]
        public void ContainsOnlyItself()
        {
            Assert.IsTrue(a.Contains(a));
            Assert.IsTrue(b.Contains(b));

            Assert.IsFalse(a.Contains(b));
            Assert.IsFalse(b.Contains(a));
        }

        [Test]
        public void YieldsOnlyItselfWhenAskedToFindTypeVariableOccurrences()
        {
            Assert.AreEqual(new[] {a}, a.FindTypeVariables().ToArray());
            Assert.AreEqual(new[] {b}, b.FindTypeVariables().ToArray());
        }

        [Test]
        public void CanPerformTypeVariableSubstitutionOnItself()
        {
            IDictionary<TypeVariable, DataType> replaceAWithInteger =
                new Dictionary<TypeVariable, DataType> { { a, NamedType.Integer } };

            Assert.AreSame(NamedType.Integer, a.ReplaceTypeVariables(replaceAWithInteger));
            Assert.AreSame(b, b.ReplaceTypeVariables(replaceAWithInteger));
        }

        [Test]
        public void ImplementsGetHashCodeAndEqualsBasedOnNameEquality()
        {
            TypeVariable c = new TypeVariable(2);
            TypeVariable a2 = new TypeVariable(0);

            Assert.IsTrue(a.Equals(a));
            Assert.IsTrue(b.Equals(b));
            Assert.IsTrue(c.Equals(c));
            Assert.IsTrue(a.Equals(a2));
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
            Assert.IsFalse(a.Equals(null));

            Assert.IsTrue(a == a);
            Assert.IsTrue(b == b);
            Assert.IsTrue(c == c);
            Assert.IsTrue(a == a2);
            Assert.IsFalse(a == b);
            Assert.IsFalse(b == a);
            Assert.IsFalse(a == null);
            Assert.IsFalse(null == a);

            Assert.IsFalse(a != a);
            Assert.IsFalse(b != b);
            Assert.IsFalse(c != c);
            Assert.IsFalse(a != a2);
            Assert.IsTrue(a != b);
            Assert.IsTrue(b != a);
            Assert.IsTrue(a != null);
            Assert.IsTrue(null != a);

            Assert.IsTrue(a.GetHashCode() == a2.GetHashCode());
            Assert.IsFalse(a.GetHashCode() == b.GetHashCode());
        }
    }
}