using NUnit.Framework;
using Parsley;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class ParentheticalSpec : ExpressionSpec
    {
        [Test]
        public void ParentheticalExpressions()
        {
            Parses("(1)").IntoTree("1");
            Parses("(a)").IntoTree("a");

            AssertType(Integer, "((1))");
        }

        [Test]
        public void DemandsBalancedParentheses()
        {
            FailsToParse("(1(", "(").WithMessage("(1, 3): ) expected");
        }
    }
}