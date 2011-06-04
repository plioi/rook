using System.Linq;
using NUnit.Framework;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class VectorLiteralSpec : ExpressionSpec
    {
        [Test]
        public void ContainsOneOrMoreExpressions()
        {
            Parser.FailsToParse("[", "");
            Parser.FailsToParse("[]", "[]");
            Parser.FailsToParse("[0", "").WithMessage("(1, 3): ] expected");
            AssertTree("[0]", "[0]");
            AssertTree("[0, 1]", "[0, 1]");
            AssertTree("[0, ((1) + (2)), 3]", "[0, 1+2, 3]");
            AssertTree("[true, ((false) || (true)), false]", "[true, false||true, false]");
        }

        [Test]
        public void FailsTypeCheckingWhenItemExpressionTypesDoNotMatch()
        {
            AssertTypeCheckError(1, 2, "Type mismatch: expected int, found bool.", "[0, true]");
        }

        [Test]
        public void HasVectorTypeBasedOnTheTypeOfItsItemExpressions()
        {
            AssertType(NamedType.Vector(Integer), "[0, 1, 2]");
            AssertType(NamedType.Vector(Boolean), "[true, false, true]");
        }

        [Test]
        public void CanCreateFullyTypedInstance()
        {
            var node = (VectorLiteral)Parse("[foo, bar]");
            node.Items.ElementAt(0).Type.ShouldBeNull();
            node.Items.ElementAt(1).Type.ShouldBeNull();
            node.Type.ShouldBeNull();

            var typedNode = (VectorLiteral)node.WithTypes(Environment(foo => Boolean, bar => Boolean)).Syntax;
            typedNode.Items.ElementAt(0).Type.ShouldBeTheSameAs(Boolean);
            typedNode.Items.ElementAt(1).Type.ShouldBeTheSameAs(Boolean);
            typedNode.Type.ShouldBeTheSameAs(NamedType.Vector(Boolean));
        }
    }
}