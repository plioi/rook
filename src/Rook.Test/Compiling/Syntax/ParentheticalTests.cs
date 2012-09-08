using Parsley;
using Should;

namespace Rook.Compiling.Syntax
{
    [Facts]
    public class ParentheticalTests : ExpressionTests
    {
        public void ParentheticalExpressions()
        {
            Parses("(1)").IntoTree("1");
            Parses("(a)").IntoTree("a");

            Type("((1))").ShouldEqual(Integer);
        }

        public void DemandsBalancedParentheses()
        {
            FailsToParse("(1(").AtEndOfInput().WithMessage("(1, 4): ) expected");
        }

        public void GroupingSupercedesBasicOperatorPrecedence()
        {
            Parses("(1+2)*3").IntoTree("((((1) + (2))) * (3))");
            Parses("1/(2+3)").IntoTree("((1) / (((2) + (3))))");
        }
    }
}