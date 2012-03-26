using Parsley;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class ParentheticalTests : ExpressionTests
    {
        [Fact]
        public void ParentheticalExpressions()
        {
            Parses("(1)").IntoTree("1");
            Parses("(a)").IntoTree("a");

            AssertType(Integer, "((1))");
        }

        [Fact]
        public void DemandsBalancedParentheses()
        {
            FailsToParse("(1(").AtEndOfInput().WithMessage("(1, 4): ) expected");
        }

        [Fact]
        public void GroupingSupercedesBasicOperatorPrecedence()
        {
            Parses("(1+2)*3").IntoTree("((((1) + (2))) * (3))");
            Parses("1/(2+3)").IntoTree("((1) / (((2) + (3))))");
        }
    }
}