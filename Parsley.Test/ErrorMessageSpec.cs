using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class ErrorMessageSpec
    {
        [Test]
        public void CanIndicateGenericErrors()
        {
            var error = new ErrorMessage();
            error.Expectation.ShouldBeNull();
        }

        [Test]
        public void CanIndicateSpecificExpectation()
        {
            var error = new ErrorMessage("statement");
            error.Expectation.ShouldEqual("statement");
        }
    }
}