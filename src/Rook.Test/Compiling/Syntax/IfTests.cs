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
            FailsToParse("if").AtEndOfInput().WithMessage("(1, 3): ( expected");
            FailsToParse("if (").AtEndOfInput();
            FailsToParse("if (false").AtEndOfInput().WithMessage("(1, 10): ) expected");
            FailsToParse("if (1)").AtEndOfInput();
            FailsToParse("if (true) 0").AtEndOfInput();
            FailsToParse("if (true) 0 1").LeavingUnparsedTokens("1");
            FailsToParse("if (true) 0 else").AtEndOfInput();
            FailsToParse("if false 0 1").LeavingUnparsedTokens("false", "0", "1").WithMessage("(1, 4): ( expected");

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
        public void FailsTypeCheckingWhenConditionExpressionFailsTypeChecking()
        {
            TypeChecking("if (a) true else false").ShouldFail("Reference to undefined identifier: a", 1, 5);
        }

        [Fact]
        public void FailsTypeCheckingWhenFirstBodyExpressionFailsTypeChecking()
        {
            TypeChecking("if (true) a else false").ShouldFail("Reference to undefined identifier: a", 1, 11);
        }

        [Fact]
        public void FailsTypeCheckingWhenSecondBodyExpressionFailsTypeChecking()
        {
            TypeChecking("if (true) true else a").ShouldFail("Reference to undefined identifier: a", 1, 21);
        }

        [Fact]
        public void FailsTypeCheckingWhenConditionExpressionIsNotBoolean()
        {
            TypeChecking("if (0) false else true").ShouldFail("Type mismatch: expected bool, found int.", 1, 1);
        }

        [Fact]
        public void FailsTypeCheckingWhenBodyExpressionTypesDoNotMatch()
        {
            TypeChecking("if (true) 0 else true").ShouldFail("Type mismatch: expected int, found bool.", 1, 1);
        }

        [Fact]
        public void HasATypeEqualToThatOfItsBodyExpressions()
        {
            Type("if (true) 1 else 0").ShouldEqual(Integer);
            Type("if (true) true else false").ShouldEqual(Boolean);
            Type("if (true) if (true) 0 else 1 else if (false) 2 else 3").ShouldEqual(Integer);
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