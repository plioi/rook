using NUnit.Framework;
using Parsley;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class ParentheticalTests : ExpressionTests
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
            FailsToParse("(1(", "").WithMessage("(1, 4): ) expected");
        }

        [Test]
        public void GroupingSupercedesBasicOperatorPrecedence()
        {
            Parses("(1+2)*3").IntoTree("((((1) + (2))) * (3))");
            Parses("1/(2+3)").IntoTree("((1) / (((2) + (3))))");
        }
    }
}