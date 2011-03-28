using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class IfSpec : ExpressionSpec
    {
        [Test]
        public void ContainsConditionExpressionAndTwoBodyExpressions()
        {
            AssertError("if", "", "(1, 3): ( expected");
            AssertError("if (", "");
            AssertError("if (false", "", "(1, 10): ) expected");
            AssertError("if (1)", "");
            AssertError("if (true) 0", "");
            AssertError("if (true) 0 1", "1");
            AssertError("if (true) 0 else", "");
            AssertError("if false 0 1", "false 0 1", "(1, 4): ( expected");

            AssertTree("(if (true) (0) else (1))", "if (true) 0 else 1");
            AssertTree("(if (false) (1) else (0))", "if (false) 1 else 0");
            AssertTree("(if (((a) || (b))) (1) else (0))", "if (a || b) 1 else 0");
        }

        [Test]
        public void AssociatesElseExpressionWithNearestPrecedingIf()
        {
            AssertTree("(if (x) ((if (y) (0) else (1))) else ((if (z) (2) else (3))))",
                       @"if (x) 
                             if (y) 
                                 0 
                             else 
                                 1
                         else 
                             if (z) 
                                 2
                             else 
                                 3");
        }

        [Test]
        public void FailsTypeCheckingWhenConditionExpressionIsNotBoolean()
        {
            AssertTypeCheckError(1, 1, "Type mismatch: expected bool, found int.", "if (0) false else true");
        }

        [Test]
        public void FailsTypeCheckingWhenBodyExpressionTypesDoNotMatch()
        {
            AssertTypeCheckError(1, 1, "Type mismatch: expected int, found bool.", "if (true) 0 else true");
        }

        [Test]
        public void HasATypeEqualToThatOfItsBodyExpressions()
        {
            AssertType(Integer, "if (true) 1 else 0");
            AssertType(Boolean, "if (true) true else false");
            AssertType(Integer, "if (true) if (true) 0 else 1 else if (false) 2 else 3");
        }

        [Test]
        public void CanCreateFullyTypedInstance()
        {
            If node = (If)Parse("if (foo) bar else baz");
            Assert.IsNull(node.Condition.Type);
            Assert.IsNull(node.BodyWhenTrue.Type);
            Assert.IsNull(node.BodyWhenFalse.Type);
            Assert.IsNull(node.Type);
            
            If typedNode = (If)node.WithTypes(Environment(foo => Boolean, bar => Boolean, baz => Boolean)).Syntax;
            Assert.AreSame(Boolean, typedNode.Condition.Type);
            Assert.AreSame(Boolean, typedNode.BodyWhenTrue.Type);
            Assert.AreSame(Boolean, typedNode.BodyWhenFalse.Type);
            Assert.AreSame(Boolean, typedNode.Type);
        }
    }
}