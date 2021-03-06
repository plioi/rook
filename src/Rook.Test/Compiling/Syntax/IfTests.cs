using Parsley;
using Should;

namespace Rook.Compiling.Syntax
{
    public class IfTests : ExpressionTests
    {
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

        public void FailsTypeCheckingWhenConditionExpressionFailsTypeChecking()
        {
            ShouldFailTypeChecking("if (a) true else false").WithError("Reference to undefined identifier: a", 1, 5);
        }

        public void FailsTypeCheckingWhenFirstBodyExpressionFailsTypeChecking()
        {
            ShouldFailTypeChecking("if (true) a else false").WithError("Reference to undefined identifier: a", 1, 11);
        }

        public void FailsTypeCheckingWhenSecondBodyExpressionFailsTypeChecking()
        {
            ShouldFailTypeChecking("if (true) true else a").WithError("Reference to undefined identifier: a", 1, 21);
        }

        public void FailsTypeCheckingWhenConditionExpressionIsNotBoolean()
        {
            ShouldFailTypeChecking("if (0) false else true").WithError("Type mismatch: expected bool, found int.", 1, 5);
        }

        public void FailsTypeCheckingWhenBodyExpressionTypesDoNotMatch()
        {
            ShouldFailTypeChecking("if (true) 0 else true").WithError("Type mismatch: expected int, found bool.", 1, 18);
        }

        public void HasATypeEqualToThatOfItsBodyExpressions()
        {
            Type("if (true) 1 else 0").ShouldEqual(Integer);
            Type("if (true) true else false").ShouldEqual(Boolean);
            Type("if (true) if (true) 0 else 1 else if (false) 2 else 3").ShouldEqual(Integer);
        }

        public void CanCreateFullyTypedInstance()
        {
            var @if = (If)Parse("if (foo) bar else baz");
            @if.Condition.Type.ShouldEqual(Unknown);
            @if.BodyWhenTrue.Type.ShouldEqual(Unknown);
            @if.BodyWhenFalse.Type.ShouldEqual(Unknown);
            @if.Type.ShouldEqual(Unknown);

            var typedIf = WithTypes(@if, foo => Boolean, bar => Boolean, baz => Boolean);
            typedIf.Condition.Type.ShouldEqual(Boolean);
            typedIf.BodyWhenTrue.Type.ShouldEqual(Boolean);
            typedIf.BodyWhenFalse.Type.ShouldEqual(Boolean);
            typedIf.Type.ShouldEqual(Boolean);
        }
    }
}