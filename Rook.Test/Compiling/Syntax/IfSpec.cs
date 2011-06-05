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
            FailsToParse("if", "").WithMessage("(1, 3): ( expected");
            FailsToParse("if (", "");
            FailsToParse("if (false", "").WithMessage("(1, 10): ) expected");
            FailsToParse("if (1)", "");
            FailsToParse("if (true) 0", "");
            FailsToParse("if (true) 0 1", "1");
            FailsToParse("if (true) 0 else", "");
            FailsToParse("if false 0 1", "false 0 1").WithMessage("(1, 4): ( expected");

            Parses("if (true) 0 else 1").IntoTree("(if (true) (0) else (1))");
            Parses("if (false) 1 else 0").IntoTree("(if (false) (1) else (0))");
            Parses("if (a || b) 1 else 0").IntoTree("(if (((a) || (b))) (1) else (0))");
        }

        [Test]
        public void AssociatesElseExpressionWithNearestPrecedingIf()
        {
            const string source =
                @"if (x) 
                    if (y) 
                      0 
                    else 
                      1
                  else 
                    if (z) 
                      2
                    else 
                      3";

            Parses(source).IntoTree("(if (x) ((if (y) (0) else (1))) else ((if (z) (2) else (3))))");
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