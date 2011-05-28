using System;
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
            lexer.CurrentToken.ShouldEqual("ABC");
            lexer.Advance().CurrentToken.ShouldEqual("def");
            lexer.Advance().Advance().CurrentToken.ShouldEqual("GHI");
            lexer.Advance().Advance().Advance().CurrentToken.ShouldBeNull();
        }

        [Test]
        public void TreatsEntireRemainingTextAsOneTokenWhenAllMatchersFail()
        {
            var lexer = new Lexer(new Text("ABCdef!ABCdef"), @"[a-z]+", @"[A-Z]+");
            lexer.CurrentToken.ShouldEqual("ABC");
            lexer.Advance().CurrentToken.ShouldEqual("def");
            lexer.Advance().Advance().CurrentToken.ShouldEqual("!ABCdef");
            lexer.Advance().Advance().Advance().CurrentToken.ShouldBeNull();
        }
    }
}