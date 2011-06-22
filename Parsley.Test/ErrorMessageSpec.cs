using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class ErrorMessageSpec
    {
        [Test]
        public void CanIndicateGenericErrors()
        {
            var error = ErrorMessage.Unknown();
            error.Expectation.ShouldBeNull();
        }

        [Test]
        public void CanIndicateSpecificExpectation()
        {
            var error = ErrorMessage.Expected("statement");
            error.Expectation.ShouldEqual("statement");
        }
    }
}