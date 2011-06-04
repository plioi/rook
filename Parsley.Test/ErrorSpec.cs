using System;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class ErrorSpec
    {
        private Text x, endOfInput;

        [SetUp]
        public void SetUp()
        {
            x = new Text("x");
            endOfInput = new Text("");
        }

        [Test]
        public void CanIndicateGenericErrors()
        {
            new Error<object>(endOfInput).Message.ShouldEqual("Parse error.");
        }

        [Test]
        public void CanIndicateErrorsWithASpecificExpectation()
        {
            new Error<object>(endOfInput, "statement").Message.ShouldEqual("statement expected");
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
            new Error<object>(x, "y").ToString().ShouldEqual("(1, 1): y expected");
        }

        [Test]
        public void HasRemainingUnparsedText()
        {
            new Error<object>(x).UnparsedText.ShouldEqual(x);
            new Error<object>(endOfInput).UnparsedText.ShouldEqual(endOfInput);
        }

        [Test]
        public void ReportsErrorState()
        {
            new Error<object>(x).IsError.ShouldBeTrue();
        }

        [Test]
        public void PropogatesItselfWithoutConsumingInputWhenAskedToParseRemainingInput()
        {
            Parser<string> shouldNotBeCalled = tokens => { throw new Exception(); };

            Parsed<string> result = new Error<object>(x, "expectation").ParseRest(o => shouldNotBeCalled);
            result.IsError.ShouldBeTrue();
            result.UnparsedText.ShouldEqual(x);
            ((Error<string>) result).Message.ShouldEqual("expectation expected");
        }
    }
}