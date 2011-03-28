using NUnit.Framework;

namespace Rook.Core
{
    [TestFixture]
    public class IdentitySpec
    {
        [Test]
        public void WrapsTheGivenValue()
        {
            Identity<string> identity = new Identity<string>("value");
            Assert.AreEqual("value", identity.Value);
        }

        [Test]
        public void AllowsMutationWhenGivenFunctionThatReturnsTheNextValue()
        {
            Identity<int> identity = new Identity<int>(0);
            Assert.AreEqual(0, identity.Value);

            identity.Update(i => i + 1);
            Assert.AreEqual(1, identity.Value);

            identity.Update(i => i + 2);
            Assert.AreEqual(3, identity.Value);
        }
    }
}