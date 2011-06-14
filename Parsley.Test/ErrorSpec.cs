using System;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class ErrorSpec
    {
        private Lexer x, endOfInput;

        [SetUp]
        public void SetUp()
        {
            x = new Lexer(new Text("x"));
            endOfInput = new Lexer(new Text(""));
        }

        [Test]
        public void CanIndicateGenericErrors()
        {
            new Error<object>(endOfInput).ErrorMessages.ToString().ShouldEqual("Parse error.");
        }

        [Test]
        public void CanIndicateErrorsWithASpecificExpectation()
        {
            new Error<object>(endOfInput, new ErrorMessage("statement")).ErrorMessages.ToString().ShouldEqual("statement expected");
        }

        [Test]
        public void CanIndicateErrorsWithMultipleExpectations()
        {
            var errors = ErrorMessageList.Empty
                .With(new ErrorMessage("A"))
                .With(new ErrorMessage("B"));

            new Error<object>(endOfInput, errors).ErrorMessages.ToString().ShouldEqual("A or B expected");
        }

        [Test]
        [ExpectedException(typeof(MemberAccessException), ExpectedMessage = "(1, 1): Parse error.")]
        public void ThrowsWhenAttemptingToGetParsedValue()
        {
            var value = new Error<object>(x).Value;
        }

        [Test]
        public void ProvidesParseErrorMessageWithPositionAsToString()
        {
            new Error<object>(x).ToString().ShouldEqual("(1, 1): Parse error.");
            new Error<object>(x, new ErrorMessage("y")).ToString().ShouldEqual("(1, 1): y expected");
        }

        [Test]
        public void HasRemainingUnparsedTokens()
        {
            new Error<object>(x).UnparsedTokens.ShouldEqual(x);
            new Error<object>(endOfInput).UnparsedTokens.ShouldEqual(endOfInput);
        }

        [Test]
        public void ReportsErrorState()
        {
            new Error<object>(x).Success.ShouldBeFalse();
        }

        [Test]
        public void PropogatesItselfWithoutConsumingInputWhenAskedToParseRemainingInput()
        {
            Parser<string> shouldNotBeCalled = tokens => { throw new Exception(); };

            Reply<string> reply = new Error<object>(x, new ErrorMessage("expectation")).ParseRest(o => shouldNotBeCalled);
            reply.Success.ShouldBeFalse();
            reply.UnparsedTokens.ShouldEqual(x);
            reply.ErrorMessages.ToString().ShouldEqual("expectation expected");
        }
    }
}