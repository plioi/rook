using System;
using NUnit.Framework;

namespace Rook.Core
{
    [TestFixture]
    public class NullableSpec
    {
        [Test]
        public void CanWrapValueTypes()
        {
            Nullable<int> zero = new Nullable<int>(0);
            Assert.AreEqual(0, zero.Value);
        }

        [Test]
        public void CanWrapReferenceTypes()
        {
            object o = new object();
            Nullable<object> nullable = new Nullable<object>(o);
            Assert.AreSame(o, nullable.Value);
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
