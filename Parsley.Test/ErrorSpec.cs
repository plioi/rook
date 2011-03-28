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
            Assert.AreEqual(null, new Error<object>(endOfInput).Expectation);
        }

        [Test]
        public void CanIndicateErrorsWithASpecificExpectation()
        {
            Assert.AreEqual("statement", new Error<object>(endOfInput, "statement").Expectation);
        }

        [Test]
        [ExpectedException(typeof(MemberAccessException), ExpectedMessage = "(1, 1): Parse error.")]
        public void ThrowsWhenAttemptingToGetParsedValue()
        {
            var value = new Error<object>(x).Value;
        }

        [Test]
        public void ProvidesParseErrorMessage()
        {
            Assert.AreEqual("Parse error.", new Error<object>(x).Message);
            Assert.AreEqual("y expected", new Error<object>(x, "y").Message);
        }

        [Test]
        public void ProvidesParseErrorMessageWithPositionAsToString()
        {
            Assert.AreEqual("(1, 1): Parse error.", new Error<object>(x).ToString());
            Assert.AreEqual("(1, 1): y expected", new Error<object>(x, "y").ToString());
        }

        [Test]
        public void HasRemainingUnparsedText()
        {
            Assert.AreSame(x, new Error<object>(x).UnparsedText);
            Assert.AreSame(endOfInput, new Error<object>(endOfInput).UnparsedText);
        }

        [Test]
        public void ReportsErrorState()
        {
            Assert.IsTrue(new Error<object>(x).IsError);
        }

        [Test]
        public void PropogatesItselfWithoutConsumingInputWhenAskedToParseRemainingInput()
        {
            Parser<string> shouldNotBeCalled = tokens => { throw new Exception(); };

            Parsed<string> result = new Error<object>(x, "expectation!").ParseRest(o => shouldNotBeCalled);
            Assert.IsTrue(result.IsError);
            Assert.AreSame(x, result.UnparsedText);
            Assert.AreEqual("expectation!", ((Error<string>) result).Expectation);
        }
    }
}