using System;
using NUnit.Framework;

namespace Rook.Core
{
    [TestFixture]
    public class NullableTests
    {
        [Test]
        public void CanWrapValueTypes()
        {
            var zero = new Nullable<int>(0);
            zero.Value.ShouldEqual(0);
        }

        [Test]
        public void CanWrapReferenceTypes()
        {
            var o = new object();
            var nullable = new Nullable<object>(o);
            nullable.Value.ShouldBeTheSameAs(o);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException),
            ExpectedMessage = "Value cannot be null.\r\nParameter name: value")]
        public void CannotWrapNullReferences()
        {
            new Nullable<object>(null);
        }
    }
}
