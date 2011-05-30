using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class LexerSpec
    {
        [Test]
        public void ProvidesNullTokenAtEndOfInput()
        {
            var lexer = new Lexer(new Text(""));
            lexer.CurrentToken.ShouldBeNull();
        }

        [Test]
        public void TryingToAdvanceBeyondInputResultsInNoMovement()
        {
            var lexer = new Lexer(new Text(""));
            lexer.ShouldBeTheSameAs(lexer.Advance());
        }

        [Test]
        public void UsesPrioritizedTokenMatchersToGetCurrentToken()
        {
            var lexer = new Lexer(new Text("ABCdefGHI"), @"[a-z]+", @"[A-Z]+");
            AssertToken(lexer.CurrentToken, "ABC", 1, 1);
            AssertToken(lexer.Advance().CurrentToken, "def", 1, 4);
            AssertToken(lexer.Advance().Advance().CurrentToken, "GHI", 1, 7);
            lexer.Advance().Advance().Advance().CurrentToken.ShouldBeNull();
        }

        [Test]
        public void TreatsEntireRemainingTextAsOneTokenWhenAllMatchersFail()
        {
            var lexer = new Lexer(new Text("ABCdef!ABCdef"), @"[a-z]+", @"[A-Z]+");
            AssertToken(lexer.CurrentToken, "ABC", 1, 1);
            AssertToken(lexer.Advance().CurrentToken, "def", 1, 4);
            AssertToken(lexer.Advance().Advance().CurrentToken, "!ABCdef", 1, 7);
            lexer.Advance().Advance().Advance().CurrentToken.ShouldBeNull();
        }

        private static void AssertToken(Token actual, string expectedLiteral, int expectedLine, int expectedColumn)
        {
            actual.Literal.ShouldEqual(expectedLiteral);
            actual.Position.Line.ShouldEqual(expectedLine);
            actual.Position.Column.ShouldEqual(expectedColumn);
        }
    }
}