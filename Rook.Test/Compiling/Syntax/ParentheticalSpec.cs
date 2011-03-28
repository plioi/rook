using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class ParentheticalSpec : ExpressionSpec
    {
        [Test]
        public void ParentheticalExpressions()
        {
            AssertTree("1", "(1)");
            AssertTree("a", "(a)");

            AssertType(Integer, "((1))");
        }

        [Test]
        public void DemandsBalancedParentheses()
        {
            AssertError("(1(", "(", "(1, 3): ) expected");
        }
    }
}