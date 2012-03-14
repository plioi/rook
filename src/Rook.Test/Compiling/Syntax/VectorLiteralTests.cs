using System.Linq;
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
            FailsToParse("[", "");
            FailsToParse("[]", "[]");
            FailsToParse("[0", "").WithMessage("(1, 3): ] expected");
            Parses("[0]").IntoTree("[0]");
            Parses("[0, 1]").IntoTree("[0, 1]");
            Parses("[0, 1+2, 3]").IntoTree("[0, ((1) + (2)), 3]");
            Parses("[true, false||true, false]").IntoTree("[true, ((false) || (true)), false]");
        }

        [Fact]
        public void FailsTypeCheckingWhenItemExpressionTypesDoNotMatch()
        {
            AssertTypeCheckError(1, 2, "Type mismatch: expected int, found bool.", "[0, true]");
        }

        [Fact]
        public void HasVectorTypeBasedOnTheTypeOfItsItemExpressions()
        {
            AssertType(NamedType.Vector(Integer), "[0, 1, 2]");
            AssertType(NamedType.Vector(Boolean), "[true, false, true]");
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var node = (VectorLiteral)Parse("[foo, bar]");
            node.Items.ElementAt(0).Type.ShouldBeNull();
            node.Items.ElementAt(1).Type.ShouldBeNull();
            node.Type.ShouldBeNull();

            var typedNode = (VectorLiteral)node.WithTypes(Environment(foo => Boolean, bar => Boolean)).Syntax;
            typedNode.Items.ElementAt(0).Type.ShouldEqual(Boolean);
            typedNode.Items.ElementAt(1).Type.ShouldEqual(Boolean);
            typedNode.Type.ShouldEqual(NamedType.Vector(Boolean));
        }
    }
}