using System.Linq;
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
            lower = new TokenKind("Lowercase", @"[a-z]+");
            upper = new TokenKind("Uppercase", @"[A-Z]+");
        }

        [Test]
        public void ProvidesCurrentToken()
        {
            var lexer = new Lexer(new Text("ABCdef"), upper);
            lexer.CurrentToken.ShouldBe(upper, "ABC", 1, 1);
        }

        [Test]
        public void AdvancesToTheNextToken()
        {
            var lexer = new Lexer(new Text("ABCdef"), upper, lower);
            lexer.Advance().CurrentToken.ShouldBe(lower, "def", 1, 4);
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
        public void CanBeEnumerated()
        {
            var tokens = new Lexer(new Text("ABCdefGHIjkl"), lower, upper).ToArray();
            tokens.Length.ShouldEqual(5);
            tokens[0].ShouldBe(upper, "ABC", 1, 1);
            tokens[1].ShouldBe(lower, "def", 1, 4);
            tokens[2].ShouldBe(upper, "GHI", 1, 7);
            tokens[3].ShouldBe(lower, "jkl", 1, 10);
            tokens[4].ShouldBe(Lexer.EndOfInput, "", 1, 13);
        }

        [Test]
        public void ProvidesTokenAtUnrecognizedInput()
        {
            var tokens = new Lexer(new Text("ABC!def"), upper, lower).ToArray();
            tokens.Length.ShouldEqual(3);
            tokens[0].ShouldBe(upper, "ABC", 1, 1);
            tokens[1].ShouldBe(Lexer.Unknown, "!def", 1, 4);
            tokens[2].ShouldBe(Lexer.EndOfInput, "", 1, 8);
        }
    }
}