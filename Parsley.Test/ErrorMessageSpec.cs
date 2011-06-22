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
            error.ToString().ShouldEqual("Parse error.");
        }

        [Test]
        public void CanIndicateSpecificExpectation()
        {
            var error = (ExpectedErrorMessage)ErrorMessage.Expected("statement");
            error.Expectation.ShouldEqual("statement");
            error.ToString().ShouldEqual("statement expected");
        }
    }
}