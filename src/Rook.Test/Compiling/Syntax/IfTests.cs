using Parsley;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class IfTests : ExpressionTests
    {
        [Fact]
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

        [Fact]
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

        [Fact]
        public void FailsTypeCheckingWhenConditionExpressionIsNotBoolean()
        {
            AssertTypeCheckError(1, 1, "Type mismatch: expected bool, found int.", "if (0) false else true");
        }

        [Fact]
        public void FailsTypeCheckingWhenBodyExpressionTypesDoNotMatch()
        {
            AssertTypeCheckError(1, 1, "Type mismatch: expected int, found bool.", "if (true) 0 else true");
        }

        [Fact]
        public void HasATypeEqualToThatOfItsBodyExpressions()
        {
            AssertType(Integer, "if (true) 1 else 0");
            AssertType(Boolean, "if (true) true else false");
            AssertType(Integer, "if (true) if (true) 0 else 1 else if (false) 2 else 3");
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var node = (If)Parse("if (foo) bar else baz");
            node.Condition.Type.ShouldBeNull();
            node.BodyWhenTrue.Type.ShouldBeNull();
            node.BodyWhenFalse.Type.ShouldBeNull();
            node.Type.ShouldBeNull();
            
            var typedNode = (If)node.WithTypes(Environment(foo => Boolean, bar => Boolean, baz => Boolean)).Syntax;
            typedNode.Condition.Type.ShouldEqual(Boolean);
            typedNode.BodyWhenTrue.Type.ShouldEqual(Boolean);
            typedNode.BodyWhenFalse.Type.ShouldEqual(Boolean);
            typedNode.Type.ShouldEqual(Boolean);
        }
    }
}