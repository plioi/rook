using Parsley;
using Rook.Compiling.Types;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class VectorLiteralTests : ExpressionTests
    {
        [Fact]
        public void ContainsOneOrMoreExpressions()
        {
            FailsToParse("[").AtEndOfInput();
            FailsToParse("[]").LeavingUnparsedTokens("[]");
            FailsToParse("[0").AtEndOfInput().WithMessage("(1, 3): ] expected");
            Parses("[0]").IntoTree("[0]");
            Parses("[0, 1]").IntoTree("[0, 1]");
            Parses("[0, 1+2, 3]").IntoTree("[0, ((1) + (2)), 3]");
            Parses("[true, false||true, false]").IntoTree("[true, ((false) || (true)), false]");
        }

        [Fact]
        public void FailsTypeCheckingWhenItemExpressionsFailTypeChecking()
        {
            TypeChecking("[a]").ShouldFail("Reference to undefined identifier: a", 1, 2);
        }

        [Fact]
        public void FailsTypeCheckingWhenItemExpressionTypesDoNotMatch()
        {
            TypeChecking("[0, 1, true]").ShouldFail("Type mismatch: expected int, found bool.", 1, 8);
        }

        [Fact]
        public void HasVectorTypeBasedOnTheTypeOfItsItemExpressions()
        {
            Type("[0, 1, 2]").ShouldEqual(NamedType.Vector(Integer));
            Type("[true, false, true]").ShouldEqual(NamedType.Vector(Boolean));
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var node = (VectorLiteral)Parse("[foo, bar]");
            node.Items.ShouldHaveTypes(null, null);
            node.Type.ShouldBeNull();

            var typedNode = (VectorLiteral)node.WithTypes(Environment(foo => Boolean, bar => Boolean)).Syntax;
            typedNode.Items.ShouldHaveTypes(Boolean, Boolean);
            typedNode.Type.ShouldEqual(NamedType.Vector(Boolean));
        }
    }
}