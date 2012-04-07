using Parsley;
using Should;
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

            Type("((1))").ShouldEqual(Integer);
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