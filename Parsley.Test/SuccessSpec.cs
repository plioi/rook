using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class SuccessSpec
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
            Parser<string> next = tokens => new Success<string>(tokens.CurrentToken.Literal, tokens.Advance());

            Parsed<string> result = new Success<string>("x", unparsed).ParseRest(s => next);
            result.IsError.ShouldBeFalse();
            result.Value.ShouldEqual("0");
            result.UnparsedTokens.CurrentToken.Kind.ShouldEqual(Lexer.EndOfInput);
        }
    }
}