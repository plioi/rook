using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class BlockTests : ExpressionTests
    {
        [Fact]
        public void ContainsOneOrMoreInnerExpressions()
        {
            FailsToParse("{}").LeavingUnparsedTokens("}");
            Parses("{1;}").IntoTree("{1;}");
            Parses("{1; true;}").IntoTree("{1; true;}");
            Parses("{1 + 2; true || false;}").IntoTree("{((1) + (2)); ((true) || (false));}");
            Parses("{(1 + 2); (true || false);}").IntoTree("{((1) + (2)); ((true) || (false));}");
        }

        [Fact]
        public void AllowsOptionalLeadingVariableDeclarations()
        {
            FailsToParse("{int 0}").LeavingUnparsedTokens("int", "0", "}");
            FailsToParse("{int x 0}").LeavingUnparsedTokens("int", "x", "0", "}");
            FailsToParse("{int x = 0}").LeavingUnparsedTokens("int", "x", "=", "0", "}");
            FailsToParse("{int x = 0 0}").LeavingUnparsedTokens("int", "x", "=", "0", "0", "}");
            FailsToParse("{int a = 0;}").LeavingUnparsedTokens("}");
            Parses("{ int a = 0; 1; }").IntoTree("{int a = 0;1;}");
            Parses("{ int a = 0; bool b = true||false; false; 0; }").IntoTree("{int a = 0; bool b = ((true) || (false));false; 0;}");
        }

        [Fact]
        public void AllowsVariableDeclarationsToOmitExplicitTypeDeclaration()
        {
            Parses("{ a = 0; 1; }").IntoTree("{a = 0;1;}");
            Parses("{ a = 0; b = true||false; false; 0; }").IntoTree("{a = 0; b = ((true) || (false));false; 0;}");
        }

        [Fact]
        public void HasATypeEqualToTheTypeOfTheLastInnerExpression()
        {
            Type("{ true; false; 1; }").ShouldEqual(Integer);
            Type("{ 1; 2; 3; 4; true; }").ShouldEqual(Boolean);
            Type("{ 1; 2; 3; 4; test; }", test => Integer).ShouldEqual(Integer);
            Type("{ 1; 2; 3; 4; test; }", test => Boolean).ShouldEqual(Boolean);
        }

        [Fact]
        public void EvaluatesBodyExpressionTypesInANewScopeIncludingLocalVariableDeclarations()
        {
            Type("{ int x = 0; x; }").ShouldEqual(Integer);
            Type("{ int x = 0; int y = 1; bool t = true; x==y || t; }").ShouldEqual(Boolean);
        }

        [Fact]
        public void EvaluatesDeclarationExpressionsInANewScopeIncludingPrecedingVariableDeclarations()
        {
            Type("{ int x = 0; int y = x+1; x+y; }").ShouldEqual(Integer);
            Type("{ int x = 0; bool b = x==0; !b; }").ShouldEqual(Boolean);
        }

        [Fact]
        public void InfersLocalVariableTypeFromInitializationExpressionTypeWhenExplicitTypeDeclarationIsOmitted()
        {
            Type("{ x = 0; x; }").ShouldEqual(Integer);
            Type("{ x = 0; y = 1; t = true; x==y || t; }").ShouldEqual(Boolean);
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var node = (Block)Parse("{ int x = y; int z = 0; xz = x>z; x; z; xz; }");
            node.VariableDeclarations.ShouldHaveTypes(Integer, Integer, null/*Implicitly typed.*/);
            node.VariableDeclarations.Select(x => x.Value).ShouldHaveTypes(null, null, null);
            node.InnerExpressions.ShouldHaveTypes(null, null, null);
            node.Type.ShouldBeNull();

            var typedNode = (Block)node.WithTypes(Scope(y => Integer), new TypeUnifier()).Syntax;
            typedNode.VariableDeclarations.ShouldHaveTypes(Integer, Integer, Boolean);
            typedNode.VariableDeclarations.Select(x => x.Value).ShouldHaveTypes(Integer, Integer, Boolean);
            typedNode.InnerExpressions.ShouldHaveTypes(Integer, Integer, Boolean);
            typedNode.Type.ShouldEqual(Boolean);
        }

        [Fact]
        public void FailsTypeCheckingWhenLocalVariableInitializationExpressionFailsTypeChecking()
        {
            TypeChecking("{ int x = a; x; }").ShouldFail("Reference to undefined identifier: a", 1, 11);
        }

        [Fact]
        public void FailsTypeCheckingWhenAnyBodyExpressionFailsTypeChecking()
        {
            TypeChecking("{ true+0; 0; }").ShouldFail("Type mismatch: expected int, found bool.", 1, 7);
        }

        [Fact]
        public void FailsTypeCheckingWhenLocalVariableNamesAreNotUnique()
        {
            TypeChecking("{ int x = 0; int y = 1; int z = 2; int x = 3; true; }").ShouldFail("Duplicate identifier: x", 1, 40);
        }

        [Fact]
        public void FailsTypeCheckingWhenLocalVariableNamesShadowSurroundingScope()
        {
            TypeChecking("{ int x = 0; int y = 1; int z = 2; true; }", z => Integer).ShouldFail("Duplicate identifier: z", 1, 29);
        }

        [Fact]
        public void FailsTypeCheckingWhenDeclaredTypeDoesNotMatchInitializationExpressionType()
        {
            TypeChecking("{ int x = false; x; }").ShouldFail("Type mismatch: expected int, found bool.", 1, 11);
        }
    }
}