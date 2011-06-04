using NUnit.Framework;
using Parsley;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class IfSpec : ExpressionSpec
    {
        [Test]
        public void ContainsConditionExpressionAndTwoBodyExpressions()
        {
            ParserUnderTest.FailsToParse("if", "").WithMessage("(1, 3): ( expected");
            ParserUnderTest.FailsToParse("if (", "");
            ParserUnderTest.FailsToParse("if (false", "").WithMessage("(1, 10): ) expected");
            ParserUnderTest.FailsToParse("if (1)", "");
            ParserUnderTest.FailsToParse("if (true) 0", "");
            ParserUnderTest.FailsToParse("if (true) 0 1", "1");
            ParserUnderTest.FailsToParse("if (true) 0 else", "");
            ParserUnderTest.FailsToParse("if false 0 1", "false 0 1").WithMessage("(1, 4): ( expected");

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
            var node = (If)Parse("if (foo) bar else baz");
            node.Condition.Type.ShouldBeNull();
            node.BodyWhenTrue.Type.ShouldBeNull();
            node.BodyWhenFalse.Type.ShouldBeNull();
            node.Type.ShouldBeNull();
            
            var typedNode = (If)node.WithTypes(Environment(foo => Boolean, bar => Boolean, baz => Boolean)).Syntax;
            typedNode.Condition.Type.ShouldBeTheSameAs(Boolean);
            typedNode.BodyWhenTrue.Type.ShouldBeTheSameAs(Boolean);
            typedNode.BodyWhenFalse.Type.ShouldBeTheSameAs(Boolean);
            typedNode.Type.ShouldBeTheSameAs(Boolean);
        }
    }
}