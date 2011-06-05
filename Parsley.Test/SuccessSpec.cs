using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class SuccessSpec
    {
        private string parsed;
        private Lexer unparsed;
        private Parsed<string> pair;

        [SetUp]
        public void SetUp()
        {
            parsed = "parsed";
            unparsed = new CharLexer("0");
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
        public void HasRemainingUnparsedTokens()
        {
            pair.UnparsedTokens.ShouldEqual(unparsed);
        }

        [Test]
        public void ReportsNonerrorState()
        {
            pair.IsError.ShouldBeFalse();
        }

        [Test]
        public void CanContinueParsingTheRemainingInputWhenGivenAParserGenerator()
        {
            Parsed<string> result = new Success<string>("x", unparsed).ParseRest(s => OneChar);
            result.IsError.ShouldBeFalse();
            result.Value.ShouldEqual("0");
            result.UnparsedTokens.EndOfInput.ShouldBeTrue();
        }

        private static readonly Parser<string> OneChar = tokens => new Success<string>(tokens.CurrentToken.Literal, tokens.Advance());
    }
}