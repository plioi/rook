using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class LexerSpec
    {
        private object lower;
        private object upper;

        [SetUp]
        public void SetUp()
        {
            lower = new object();
            upper = new object();
        }

        [Test]
        public void ProvidesTokenAtEndOfInput()
        {
            var lexer = new Lexer(new Text(""));
            lexer.CurrentToken.AssertToken(TokenKind.EndOfInput, "", 1, 1);
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
            var lexer = new Lexer(new Text("ABCdefGHI"),
                                  new TokenMatcher(lower, @"[a-z]+"),
                                  new TokenMatcher(upper, @"[A-Z]+"));
            lexer.CurrentToken.AssertToken(upper, "ABC", 1, 1);
            lexer.Advance().CurrentToken.AssertToken(lower, "def", 1, 4);
            lexer.Advance().Advance().CurrentToken.AssertToken(upper, "GHI", 1, 7);
            lexer.Advance().Advance().Advance().CurrentToken.AssertToken(TokenKind.EndOfInput, "", 1, 10);
        }

        [Test]
        public void TreatsEntireRemainingTextAsOneTokenWhenAllMatchersFail()
        {
            var lexer = new Lexer(new Text("ABCdef!ABCdef"),
                                  new TokenMatcher(lower, @"[a-z]+"),
                                  new TokenMatcher(upper, @"[A-Z]+"));
            lexer.CurrentToken.AssertToken(upper, "ABC", 1, 1);
            lexer.Advance().CurrentToken.AssertToken(lower, "def", 1, 4);
            lexer.Advance().Advance().CurrentToken.AssertToken(TokenKind.Unknown, "!ABCdef", 1, 7);
            lexer.Advance().Advance().Advance().CurrentToken.AssertToken(TokenKind.EndOfInput, "", 1, 14);
        }
    }
}