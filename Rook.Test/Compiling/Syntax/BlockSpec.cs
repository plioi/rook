using System.Linq;
using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class BlockSpec : ExpressionSpec
    {
        [Test]
        public void ContainsOneOrMoreInnerExpressions()
        {
            FailsToParse("{}", "}");
            Parses("{1;}").IntoTree("{1;}");
            Parses("{1; true;}").IntoTree("{1; true;}");
            Parses("{1 + 2; true || false;}").IntoTree("{((1) + (2)); ((true) || (false));}");
            Parses("{(1 + 2); (true || false);}").IntoTree("{((1) + (2)); ((true) || (false));}");
        }

        [Test]
        public void AllowsOptionalLeadingVariableDeclarations()
        {
            FailsToParse("{int 0}", "int 0}");
            FailsToParse("{int x 0}", "int x 0}");
            FailsToParse("{int x = 0}", "int x = 0}");
            FailsToParse("{int x = 0 0}", "int x = 0 0}");
            FailsToParse("{int a = 0;}", "}");
            Parses("{ int a = 0; 1; }").IntoTree("{int a = 0;1;}");
            Parses("{ int a = 0; bool b = true||false; false; 0; }").IntoTree("{int a = 0; bool b = ((true) || (false));false; 0;}");
        }

        [Test]
        public void AllowsVariableDeclarationsToOmitExplicitTypeDeclaration()
        {
            Parses("{ a = 0; 1; }").IntoTree("{a = 0;1;}");
            Parses("{ a = 0; b = true||false; false; 0; }").IntoTree("{a = 0; b = ((true) || (false));false; 0;}");
        }

        [Test]
        public void HasATypeEqualToTheTypeOfTheLastInnerExpression()
        {
            AssertType(Integer, "{ true; false; 1; }");
            AssertType(Boolean, "{ 1; 2; 3; 4; true; }");
            AssertType(Integer, "{ 1; 2; 3; 4; test; }", test => Integer);
            AssertType(Boolean, "{ 1; 2; 3; 4; test; }", test => Boolean);
        }

        [Test]
        public void EvaluatesBodyExpressionTypesInANewScopeIncludingLocalVariableDeclarations()
        {
            AssertType(Integer, "{ int x = 0; x; }");
            AssertType(Boolean, "{ int x = 0; int y = 1; bool t = true; x==y || t; }");
        }

        [Test]
        public void EvaluatesDeclarationExpressionsInANewScopeIncludingPrecedingVariableDeclarations()
        {
            AssertType(Integer, "{ int x = 0; int y = x+1; x+y; }");
            AssertType(Boolean, "{ int x = 0; bool b = x==0; !b; }");
        }

        [Test]
        public void InfersLocalVariableTypeFromInitializationExpressionTypeWhenExplicitTypeDeclarationIsOmitted()
        {
            AssertType(Integer, "{ x = 0; x; }");
            AssertType(Boolean, "{ x = 0; y = 1; t = true; x==y || t; }");
        }

        [Test]
        public void CanCreateFullyTypedInstance()
        {
            var node = (Block)Parse("{ int x = y; int z = 0; xz = x>z; x; z; xz; }");
            node.VariableDeclarations.ElementAt(0).Type.ShouldBeTheSameAs(Integer);
            node.VariableDeclarations.ElementAt(0).Value.Type.ShouldBeNull();
            node.VariableDeclarations.ElementAt(1).Type.ShouldBeTheSameAs(Integer);
            node.VariableDeclarations.ElementAt(1).Value.Type.ShouldBeTheSameAs(Integer);//Integer constants are always typed.
            node.VariableDeclarations.ElementAt(2).Type.ShouldBeNull();//Implicitly typed.
            node.VariableDeclarations.ElementAt(2).Value.Type.ShouldBeNull();
            node.InnerExpressions.ElementAt(0).Type.ShouldBeNull();
            node.InnerExpressions.ElementAt(1).Type.ShouldBeNull();
            node.InnerExpressions.ElementAt(2).Type.ShouldBeNull();
            node.Type.ShouldBeNull();

            var typedNode = (Block)node.WithTypes(Environment(y => Integer)).Syntax;
            typedNode.VariableDeclarations.ElementAt(0).Type.ShouldBeTheSameAs(Integer);
            typedNode.VariableDeclarations.ElementAt(0).Value.Type.ShouldBeTheSameAs(Integer);
            typedNode.VariableDeclarations.ElementAt(1).Type.ShouldBeTheSameAs(Integer);
            typedNode.VariableDeclarations.ElementAt(1).Value.Type.ShouldBeTheSameAs(Integer);
            typedNode.VariableDeclarations.ElementAt(2).Type.ShouldBeTheSameAs(Boolean);
            typedNode.VariableDeclarations.ElementAt(2).Value.Type.ShouldBeTheSameAs(Boolean);
            typedNode.InnerExpressions.ElementAt(0).Type.ShouldBeTheSameAs(Integer);
            typedNode.InnerExpressions.ElementAt(1).Type.ShouldBeTheSameAs(Integer);
            typedNode.InnerExpressions.ElementAt(2).Type.ShouldBeTheSameAs(Boolean);
            typedNode.Type.ShouldBeTheSameAs(Boolean);
        }

        [Test]
        public void FailsTypeCheckingWhenAnyBodyExpressionFailsTypeChecking()
        {
            AssertTypeCheckError(1, 7, "Type mismatch: expected int, found bool.", "{ true+0; 0; }");
        }

        [Test]
        public void FailsTypeCheckingWhenLocalVariableNamesAreNotUnique()
        {
            AssertTypeCheckError(1, 40, "Duplicate identifier: x", "{ int x = 0; int y = 1; int z = 2; int x = 3; true; }");
        }

        [Test]
        public void FailsTypeCheckingWhenLocalVariableNamesShadowSurroundingScope()
        {
            AssertTypeCheckError(1, 29, "Duplicate identifier: z", "{ int x = 0; int y = 1; int z = 2; true; }", z => Integer);
        }

        [Test]
        public void FailsTypeCheckingWhenDeclaredTypeDoesNotMatchInitializationExpressionType()
        {
            AssertTypeCheckError(1, 11, "Type mismatch: expected int, found bool.", "{ int x = false; x; }");
        }
    }
}