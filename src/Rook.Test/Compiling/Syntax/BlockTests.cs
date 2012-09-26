using System.Linq;
using Parsley;
using Should;

namespace Rook.Compiling.Syntax
{
    [Facts]
    public class BlockTests : ExpressionTests
    {
        public void ContainsOneOrMoreInnerExpressions()
        {
            FailsToParse("{}").LeavingUnparsedTokens("}");
            Parses("{1;}").IntoTree("{1;}");
            Parses("{1; true;}").IntoTree("{1; true;}");
            Parses("{1 + 2; true || false;}").IntoTree("{((1) + (2)); ((true) || (false));}");
            Parses("{(1 + 2); (true || false);}").IntoTree("{((1) + (2)); ((true) || (false));}");
        }

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

        public void AllowsVariableDeclarationsToOmitExplicitTypeDeclaration()
        {
            Parses("{ a = 0; 1; }").IntoTree("{a = 0;1;}");
            Parses("{ a = 0; b = true||false; false; 0; }").IntoTree("{a = 0; b = ((true) || (false));false; 0;}");
        }

        public void HasATypeEqualToTheTypeOfTheLastInnerExpression()
        {
            Type("{ true; false; 1; }").ShouldEqual(Integer);
            Type("{ 1; 2; 3; 4; true; }").ShouldEqual(Boolean);
            Type("{ 1; 2; 3; 4; test; }", test => Integer).ShouldEqual(Integer);
            Type("{ 1; 2; 3; 4; test; }", test => Boolean).ShouldEqual(Boolean);
        }

        public void EvaluatesBodyExpressionTypesInANewScopeIncludingLocalVariableDeclarations()
        {
            Type("{ int x = 0; x; }").ShouldEqual(Integer);
            Type("{ int x = 0; int y = 1; bool t = true; x==y || t; }").ShouldEqual(Boolean);
        }

        public void EvaluatesDeclarationExpressionsInANewScopeIncludingPrecedingVariableDeclarations()
        {
            Type("{ int x = 0; int y = x+1; x+y; }").ShouldEqual(Integer);
            Type("{ int x = 0; bool b = x==0; !b; }").ShouldEqual(Boolean);
        }

        public void InfersLocalVariableTypeFromInitializationExpressionTypeWhenExplicitTypeDeclarationIsOmitted()
        {
            Type("{ x = 0; x; }").ShouldEqual(Integer);
            Type("{ x = 0; y = 1; t = true; x==y || t; }").ShouldEqual(Boolean);
        }

        public void CanCreateFullyTypedInstance()
        {
            var block = (Block)Parse("{ int x = y; int z = 0; xz = x>z; x; z; xz; }");
            block.VariableDeclarations.ShouldHaveTypes(Unknown, Unknown, Unknown/*Implicitly typed.*/);
            block.VariableDeclarations.Select(x => x.Value).ShouldHaveTypes(Unknown, Unknown, Unknown);
            block.InnerExpressions.ShouldHaveTypes(Unknown, Unknown, Unknown);
            block.Type.ShouldEqual(Unknown);

            var typedBlock = WithTypes(block, y => Integer);
            typedBlock.VariableDeclarations.ShouldHaveTypes(Integer, Integer, Boolean);
            typedBlock.VariableDeclarations.Select(x => x.Value).ShouldHaveTypes(Integer, Integer, Boolean);
            typedBlock.InnerExpressions.ShouldHaveTypes(Integer, Integer, Boolean);
            typedBlock.Type.ShouldEqual(Boolean);
        }

        public void FailsTypeCheckingWhenLocalVariableInitializationExpressionFailsTypeChecking()
        {
            ShouldFailTypeChecking("{ int x = a; x; }").WithError("Reference to undefined identifier: a", 1, 11);
        }

        public void FailsTypeCheckingWhenAnyBodyExpressionFailsTypeChecking()
        {
            ShouldFailTypeChecking("{ true+0; 0; }").WithError("Type mismatch: expected int, found bool.", 1, 7);
        }

        public void FailsTypeCheckingWhenLocalVariableNamesAreNotUnique()
        {
            ShouldFailTypeChecking("{ int x = 0; int y = 1; int z = 2; int x = 3; true; }").WithError("Duplicate identifier: x", 1, 40);
        }

        public void FailsTypeCheckingWhenLocalVariableNamesShadowSurroundingScope()
        {
            ShouldFailTypeChecking("{ int x = 0; int y = 1; int z = 2; true; }", z => Integer).WithError("Duplicate identifier: z", 1, 29);
        }

        public void FailsTypeCheckingWhenDeclaredTypeDoesNotMatchInitializationExpressionType()
        {
            ShouldFailTypeChecking("{ int x = false; x; }").WithError("Type mismatch: expected int, found bool.", 1, 11);
        }
    }
}