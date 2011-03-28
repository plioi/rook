using System.Linq;
using NUnit.Framework;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class LambdaSpec : ExpressionSpec
    {
        [Test]
        public void HasABodyExpression()
        {
            AssertError("fn", "", "(1, 3): ( expected");
            AssertError("fn (", "", "(1, 5): ) expected");
            AssertError("fn ()", "");

            AssertTree("fn () 1", "fn () 1");
            AssertTree("fn () ((1) + (2))", "fn () 1 + 2");
        }

        [Test]
        public void HasAParameterList()
        {
            AssertTree("fn (int x) 1", "fn (int x) 1");
            AssertTree("fn (bool x) x", "fn (bool x) x");
            AssertTree("fn (int x, int y) ((x) + (y))", "fn (int x, int y) x+y");
        }

        [Test]
        public void AllowsParametersToOmitExplicitTypeDeclaration()
        {
            AssertTree("fn (x) 1", "fn (x) 1");
            AssertTree("fn (x, bool y) 1", "fn (x, bool y) 1");
            AssertTree("fn (int x, y, z) 1", "fn (int x, y, z) 1");
        }

        [Test]
        public void HasAFunctionTypeWithReturnTypeEqualToTheTypeOfTheBodyExpression()
        {
            AssertType(NamedType.Function(Integer), "fn () 1");
            AssertType(NamedType.Function(Boolean), "fn () true");
            AssertType(NamedType.Function(Integer), "fn () x", x => Integer);
            AssertType(NamedType.Function(Boolean), "fn () x", x => Boolean);
            AssertType(NamedType.Function(new[] { Integer }, Integer), "fn (int x) x+1");
            AssertType(NamedType.Function(new[] { Integer }, Boolean), "fn (int x) x+1 > 0");
        }

        [Test]
        public void InfersParameterTypesFromUsages()
        {
            AssertType(NamedType.Function(new[] { Integer }, Integer), "fn (x) x+1");
            AssertType(NamedType.Function(new[] { Integer, Integer }, Integer), "fn (x, y) x+y");
            AssertType(NamedType.Function(new[] { Integer }, Boolean), "fn (x) x+1 > 0");
        }

        [Test]
        public void CanCreateFullyTypedInstance()
        {
            Lambda node = (Lambda)Parse("fn (x, int y, bool z) x+y>0 && z");
            Assert.IsNull(node.Parameters.ElementAt(0).Type);
            Assert.AreSame(Integer, node.Parameters.ElementAt(1).Type);
            Assert.AreSame(Boolean, node.Parameters.ElementAt(2).Type);
            Assert.IsNull(node.Body.Type);
            Assert.IsNull(node.Type);

            Lambda typedNode = (Lambda)node.WithTypes(Environment()).Syntax;
            Assert.AreSame(Integer, typedNode.Parameters.ElementAt(0).Type);
            Assert.AreSame(Integer, typedNode.Parameters.ElementAt(1).Type);
            Assert.AreSame(Boolean, typedNode.Parameters.ElementAt(2).Type);
            Assert.AreSame(Boolean, typedNode.Body.Type);
            Assert.AreSame(NamedType.Function(new[] { Integer, Integer, Boolean }, Boolean), typedNode.Type);
        }

        [Test]
        public void FailsTypeCheckingWhenBodyExpressionFailsTypeChecking()
        {
            AssertTypeCheckError(1, 11, "Type mismatch: expected int, found bool.", "fn () true+0");
        }

        [Test]
        public void FailsTypeCheckingWhenParameterNamesAreNotUnique()
        {
            AssertTypeCheckError(1, 32, "Duplicate identifier: x", "fn (int x, bool y, int z, bool x) 0");
        }

        [Test]
        public void FailsTypeCheckingWhenParameterNamesShadowSurroundingScope()
        {
            AssertTypeCheckError(1, 24, "Duplicate identifier: z", "fn (int x, bool y, int z) 0", z => Integer);
        }
    }
}