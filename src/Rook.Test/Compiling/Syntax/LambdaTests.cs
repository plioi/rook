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
            Type("fn () 1").ShouldEqual(NamedType.Function(Integer));
            Type("fn () true").ShouldEqual(NamedType.Function(Boolean));
            Type("fn () x", x => Integer).ShouldEqual(NamedType.Function(Integer));
            Type("fn () x", x => Boolean).ShouldEqual(NamedType.Function(Boolean));
            Type("fn (int x) x+1").ShouldEqual(NamedType.Function(new[] { Integer }, Integer));
            Type("fn (int x) x+1 > 0").ShouldEqual(NamedType.Function(new[] { Integer }, Boolean));
        }

        [Fact]
        public void InfersParameterTypesFromUsages()
        {
            Type("fn (x) x+1").ShouldEqual(NamedType.Function(new[] { Integer }, Integer));
            Type("fn (x, y) x+y").ShouldEqual(NamedType.Function(new[] { Integer, Integer }, Integer));
            Type("fn (x) x+1 > 0").ShouldEqual(NamedType.Function(new[] { Integer }, Boolean));
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var node = (Lambda)Parse("fn (x, int y, bool z) x+y>0 && z");
            node.Parameters.ShouldHaveTypes(null, Integer, Boolean);
            node.Body.Type.ShouldBeNull();
            node.Type.ShouldBeNull();

            var typedNode = (Lambda)node.WithTypes(Scope(), new TypeUnifier()).Syntax;
            typedNode.Parameters.ShouldHaveTypes(Integer, Integer, Boolean);
            typedNode.Body.Type.ShouldEqual(Boolean);
            typedNode.Type.ShouldEqual(NamedType.Function(new[] { Integer, Integer, Boolean }, Boolean));
        }

        [Fact]
        public void FailsTypeCheckingWhenBodyExpressionFailsTypeChecking()
        {
            TypeChecking("fn () true+0").ShouldFail("Type mismatch: expected int, found bool.", 1, 11);
        }

        [Fact]
        public void FailsTypeCheckingWhenParameterNamesAreNotUnique()
        {
            TypeChecking("fn (int x, bool y, int z, bool x) 0").ShouldFail("Duplicate identifier: x", 1, 32);
        }

        [Fact]
        public void FailsTypeCheckingWhenParameterNamesShadowSurroundingScope()
        {
            TypeChecking("fn (int x, bool y, int z) 0", z => Integer).ShouldFail("Duplicate identifier: z", 1, 24);
        }
    }
}