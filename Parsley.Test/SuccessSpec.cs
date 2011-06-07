using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class SuccessSpec
    {
        private Lexer unparsed;

        [SetUp]
        public void SetUp()
        {
            unparsed = new CharLexer("0");
        }

        [Test]
        public void HasAParsedValue()
        {
            new Success<string>("parsed", unparsed).Value.ShouldEqual("parsed");
        }

        [Test]
        public void ProvidesParseSuccessMessage()
        {
            new Success<string>("x", unparsed).Message.ShouldEqual("Parse succeeded.");
        }

        [Test]
        public void HasRemainingUnparsedTokens()
        {
            new Success<string>("parsed", unparsed).UnparsedTokens.ShouldEqual(unparsed);
        }

        [Test]
        public void ReportsNonerrorState()
        {
            new Success<string>("parsed", unparsed).IsError.ShouldBeFalse();
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