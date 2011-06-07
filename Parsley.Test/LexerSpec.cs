using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class LexerSpec
    {
        private TokenKind lower;
        private TokenKind upper;

        [SetUp]
        public void SetUp()
        {
            lower = new TokenKind(@"[a-z]+");
            upper = new TokenKind(@"[A-Z]+");
        }

        [Test]
        public void ProvidesTokenAtEndOfInput()
        {
            var lexer = new Lexer(new Text(""));
            lexer.CurrentToken.ShouldBe(Lexer.EndOfInput, "", 1, 1);
        }

        [Test]
        public void TryingToAdvanceBeyondEndOfInputResultsInNoMovement()
        {
            var lexer = new Lexer(new Text(""));
            lexer.ShouldBeTheSameAs(lexer.Advance());
        }

        [Test]
        public void UsesPrioritizedTokenMatchersToGetCurrentToken()
        {
            var lexer = new Lexer(new Text("ABCdefGHI"), lower, upper);
            lexer.CurrentToken.ShouldBe(upper, "ABC", 1, 1);
            lexer.Advance().CurrentToken.ShouldBe(lower, "def", 1, 4);
            lexer.Advance().Advance().CurrentToken.ShouldBe(upper, "GHI", 1, 7);
            lexer.Advance().Advance().Advance().CurrentToken.ShouldBe(Lexer.EndOfInput, "", 1, 10);
        }

        [Test]
        public void TreatsEntireRemainingTextAsOneTokenWhenAllMatchersFail()
        {
            var lexer = new Lexer(new Text("ABCdef!ABCdef"), lower, upper);
            lexer.CurrentToken.ShouldBe(upper, "ABC", 1, 1);
            lexer.Advance().CurrentToken.ShouldBe(lower, "def", 1, 4);
            lexer.Advance().Advance().CurrentToken.ShouldBe(Lexer.Unknown, "!ABCdef", 1, 7);
            lexer.Advance().Advance().Advance().CurrentToken.ShouldBe(Lexer.EndOfInput, "", 1, 14);
        }
    }
}