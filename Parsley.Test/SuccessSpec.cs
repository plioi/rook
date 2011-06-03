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
            pair.Value.ShouldEqual(parsed);
        }

        [Test]
        public void ProvidesParseSuccessMessage()
        {
            new Success<string>("x", unparsed).Message.ShouldEqual("Parse succeeded.");
        }

        [Test]
        public void HasRemainingUnparsedText()
        {
            pair.UnparsedText.ShouldEqual(unparsed);
        }

        [Test]
        public void ReportsNonerrorState()
        {
            pair.IsError.ShouldBeFalse();
        }

        [Test]
        public void CanContinueParsingTheRemainingInputWhenGivenAParserGenerator()
        {
            Parsed<string> result = new Success<string>("x", unparsed).ParseRest(s => AbstractGrammar.Pattern("0"));
            result.IsError.ShouldBeFalse();
            result.Value.ShouldEqual("0");
            result.UnparsedText.EndOfInput.ShouldBeTrue();
        }
    }
}