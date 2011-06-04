using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class TokenMatcherSpec
    {
        private object lower;
        private object upper;
        private Text abcDEF;

        [SetUp]
        public void SetUp()
        {
            lower = new object();
            upper = new object();
            abcDEF = new Text("abcDEF");
        }

        [Test]
        public void ProducesNullTokenUponFailedPatternMatch()
        {
            Token token;

            new TokenMatcher(upper, @"[A-Z]+").TryMatch(abcDEF, out token).ShouldBeFalse();
            token.ShouldBeNull();
        }

        [Test]
        public void ProducesTokenUponSuccessfulPatternMatch()
        {
            Token token;

            new TokenMatcher(lower, @"[a-z]+").TryMatch(abcDEF, out token).ShouldBeTrue();
            token.ShouldBe(lower, "abc", 1, 1);

            new TokenMatcher(upper, @"[A-Z]+").TryMatch(abcDEF.Advance(3), out token).ShouldBeTrue();
            token.ShouldBe(upper, "DEF", 1, 4);
        }
    }
}