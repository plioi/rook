using Should;
using Xunit;

namespace Rook.Core
{
    public class IdentityTests
    {
        [Fact]
        public void WrapsTheGivenValue()
        {
            var identity = new Identity<string>("value");
            identity.Value.ShouldEqual("value");
        }

        [Fact]
        public void AllowsMutationWhenGivenFunctionThatReturnsTheNextValue()
        {
            var identity = new Identity<int>(0);
            identity.Value.ShouldEqual(0);

            identity.Update(i => i + 1);
            identity.Value.ShouldEqual(1);

            identity.Update(i => i + 2);
            identity.Value.ShouldEqual(3);
        }
    }
}