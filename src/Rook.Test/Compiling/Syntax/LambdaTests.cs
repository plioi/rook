using Parsley;
using Rook.Compiling.Types;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class LambdaTests : ExpressionTests
    {
        [Fact]
        public void HasABodyExpression()
        {
            FailsToParse("fn").AtEndOfInput().WithMessage("(1, 3): ( expected");
            FailsToParse("fn (").AtEndOfInput().WithMessage("(1, 5): ) expected");
            FailsToParse("fn ()").AtEndOfInput();

            Parses("fn () 1").IntoTree("fn () 1");
            Parses("fn () 1 + 2").IntoTree("fn () ((1) + (2))");
        }

        [Fact]
        public void HasAParameterList()
        {
            Parses("fn (int x) 1").IntoTree("fn (int x) 1");
            Parses("fn (bool x) x").IntoTree("fn (bool x) x");
            Parses("fn (int x, int y) x+y").IntoTree("fn (int x, int y) ((x) + (y))");
        }

        [Fact]
        public void AllowsParametersToOmitExplicitTypeDeclaration()
        {
            Parses("fn (x) 1").IntoTree("fn (x) 1");
            Parses("fn (x, bool y) 1").IntoTree("fn (x, bool y) 1");
            Parses("fn (int x, y, z) 1").IntoTree("fn (int x, y, z) 1");
        }

        [Fact]
        public void HasAFunctionTypeWithReturnTypeEqualToTheTypeOfTheBodyExpression()
        {
            AssertType(NamedType.Function(Integer), "fn () 1");
            AssertType(NamedType.Function(Boolean), "fn () true");
            AssertType(NamedType.Function(Integer), "fn () x", x => Integer);
            AssertType(NamedType.Function(Boolean), "fn () x", x => Boolean);
            AssertType(NamedType.Function(new[] { Integer }, Integer), "fn (int x) x+1");
            AssertType(NamedType.Function(new[] { Integer }, Boolean), "fn (int x) x+1 > 0");
        }

        [Fact]
        public void InfersParameterTypesFromUsages()
        {
            AssertType(NamedType.Function(new[] { Integer }, Integer), "fn (x) x+1");
            AssertType(NamedType.Function(new[] { Integer, Integer }, Integer), "fn (x, y) x+y");
            AssertType(NamedType.Function(new[] { Integer }, Boolean), "fn (x) x+1 > 0");
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var node = (Lambda)Parse("fn (x, int y, bool z) x+y>0 && z");
            node.Parameters.ShouldHaveTypes(null, Integer, Boolean);
            node.Body.Type.ShouldBeNull();
            node.Type.ShouldBeNull();

            var typedNode = (Lambda)node.WithTypes(Environment()).Syntax;
            typedNode.Parameters.ShouldHaveTypes(Integer, Integer, Boolean);
            typedNode.Body.Type.ShouldEqual(Boolean);
            typedNode.Type.ShouldEqual(NamedType.Function(new[] { Integer, Integer, Boolean }, Boolean));
        }

        [Fact]
        public void FailsTypeCheckingWhenBodyExpressionFailsTypeChecking()
        {
            AssertTypeCheckError(1, 11, "Type mismatch: expected int, found bool.", "fn () true+0");
        }

        [Fact]
        public void FailsTypeCheckingWhenParameterNamesAreNotUnique()
        {
            AssertTypeCheckError(1, 32, "Duplicate identifier: x", "fn (int x, bool y, int z, bool x) 0");
        }

        [Fact]
        public void FailsTypeCheckingWhenParameterNamesShadowSurroundingScope()
        {
            AssertTypeCheckError(1, 24, "Duplicate identifier: z", "fn (int x, bool y, int z) 0", z => Integer);
        }
    }
}