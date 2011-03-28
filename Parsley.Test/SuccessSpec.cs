using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class SuccessSpec
    {
        private string parsed;
        private Text unparsed;
        private Parsed<string> pair;

        [SetUp]
        public void SetUp()
        {
            parsed = "parsed";
            unparsed = new Text("0");
            pair = new Success<string>(parsed, unparsed);
        }

        [Test]
        public void HasAParsedValue()
        {
            Assert.AreSame(parsed, pair.Value);
        }

        [Test]
        public void ProvidesParseSuccessMessage()
        {
            Assert.AreEqual("Parse succeeded.", new Success<string>("x", unparsed).Message);
        }

        [Test]
        public void HasRemainingUnparsedText()
        {
            Assert.AreSame(unparsed, pair.UnparsedText);
        }

        [Test]
        public void ReportsNonerrorState()
        {
            Assert.IsFalse(pair.IsError);
        }

        [Test]
        public void CanContinueParsingTheRemainingInputWhenGivenAParserGenerator()
        {
            Parsed<string> result = new Success<string>("x", unparsed).ParseRest(s => AbstractGrammar.String("0"));
            Assert.IsFalse(result.IsError);
            Assert.AreEqual("0", result.Value);
            Assert.IsTrue(result.UnparsedText.EndOfInput);
        }
    }
}