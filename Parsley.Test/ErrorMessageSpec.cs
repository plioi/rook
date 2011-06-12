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
            error.ToString().ShouldEqual("Parse error.");
        }

        [Test]
        public void CanIndicateErrorsWithASpecificExpectation()
        {
            var error = new ErrorMessage("statement");
            error.ToString().ShouldEqual("statement expected");
        }
    }
}