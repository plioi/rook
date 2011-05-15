using System.Linq;
using NUnit.Framework;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class FunctionSpec : SyntaxTreeSpec<Function>
    {
        protected override Parser<Function> ParserUnderTest { get { return Grammar.Function; } }

        [Test]
        public void DemandsCompleteFunctionDefinition()
        {
            AssertError("", "", "(1, 1): type name expected");
            AssertError("int", "");
            AssertError("int foo", "", "(1, 8): ( expected");
            AssertError("int foo(", "", "(1, 9): ) expected");
            AssertError("int foo(x)", "");
            AssertError("int foo(int)", "int)", "(1, 9): ) expected");
            AssertError("int foo()", "");
            AssertTree("int foo() 1", "int foo() 1");
        }

        [Test]
        public void ParsesReturnTypes()
        {
            AssertTree("int foo() 1", "int foo() 1");
            AssertTree("bool foo() 1", "bool foo() 1");
        }

        [Test]
        public void ParsesNames()
        {
            AssertTree("int foo() 1", "int foo() 1");
            AssertTree("int bar() 1", "int bar() 1");
        }

        [Test]
        public void ParsesParameterLists()
        {
            AssertTree("int foo() 1", "int foo() 1");
            AssertTree("int foo(int x) 1", "int foo(int x) 1");
            AssertTree("int foo(int x, bool y) 1", "int foo(int x, bool y) 1");
            AssertTree("int foo(int x, bool y, int z) 1", "int foo(int x, bool y, int z) 1");
        }

        [Test]
        public void AllowsParametersToOmitExplicitTypeDeclaration()
        {
            AssertTree("int foo(x) 1", "int foo(x) 1");
            AssertTree("int foo(x, bool y) 1", "int foo(x, bool y) 1");
            AssertTree("int foo(int x, y, z) 1", "int foo(int x, y, z) 1");
        }

        [Test]
        public void ParsesBodyExpression()
        {
            AssertTree("int foo() 1", "int foo() 1");
            AssertTree("int foo() false", "int foo() false");
        }

        [Test]
        public void HasATypeIncludingInputTypesAndReturnType()
        {
            Type("int foo() 1").ShouldEqual(NamedType.Function(Integer));
            Type("bool foo(int x) false").ShouldEqual(NamedType.Function(new[] {Integer}, Boolean));
            Type("int foo(int x, bool y) 1").ShouldEqual(NamedType.Function(new[] {Integer, Boolean}, Integer));
        }

        [Test]
        public void EvaluatesBodyExpressionTypesInANewScopeIncludingParameters()
        {
            Type("int foo(int x) x").ShouldEqual(NamedType.Function(new[] {Integer}, Integer));
            Type("bool foo(int x, int y, bool b) x==y || b").ShouldEqual(
                NamedType.Function(new[] {Integer, Integer, Boolean}, Boolean));
        }

        [Test]
        public void CanCreateFullyTypedInstance()
        {
            Function node = Parse("bool foo(int x, int y, bool z) x+y>0 && z");
            node.Parameters.ElementAt(0).Type.ShouldBeTheSameAs(Integer);
            node.Parameters.ElementAt(1).Type.ShouldBeTheSameAs(Integer);
            node.Parameters.ElementAt(2).Type.ShouldBeTheSameAs(Boolean);
            node.Body.Type.ShouldBeNull();
            node.Type.ShouldBeNull();

            Function typedNode = node.WithTypes(Environment()).Syntax;
            typedNode.Parameters.ElementAt(0).Type.ShouldBeTheSameAs(Integer);
            typedNode.Parameters.ElementAt(1).Type.ShouldBeTheSameAs(Integer);
            typedNode.Parameters.ElementAt(2).Type.ShouldBeTheSameAs(Boolean);
            typedNode.Body.Type.ShouldBeTheSameAs(Boolean);
            typedNode.Type.ShouldBeTheSameAs(NamedType.Function(new[] { Integer, Integer, Boolean }, Boolean));
        }

        [Test]
        public void FailsTypeCheckingWhenBodyExpressionFailsTypeChecking()
        {
            AssertTypeCheckError(1, 15, "Type mismatch: expected int, found bool.", "int foo() true+0");
        }

        [Test]
        public void FailsTypeCheckingWhenParameterNamesAreNotUnique()
        {
            AssertTypeCheckError(1, 34, "Duplicate identifier: x", "int foo(int x, int y, int z, int x) true");
        }

        [Test]
        public void FailsTypeCheckingWhenParameterNamesShadowSurroundingScope()
        {
            AssertTypeCheckError(1, 27, "Duplicate identifier: z", "int foo(int x, int y, int z) true;", z => Integer);
        }

        [Test]
        public void FailsTypeCheckingWhenDeclaredReturnTypeDoesNotMatchBodyExpressionType()
        {
            AssertTypeCheckError(1, 5, "Type mismatch: expected int, found bool.", "int foo (int x) false");
        }

        private DataType Type(string source, params TypeMapping[] symbols)
        {
            return TypeCheck(source, symbols).Syntax.Type;
        }

        private TypeChecked<Function> TypeCheck(string source, TypeMapping[] symbols)
        {
            return Parse(source).WithTypes(Environment(symbols));
        }

        private void AssertTypeCheckError(int line, int column, string expectedMessage, string source,
                                          params TypeMapping[] symbols)
        {
            AssertTypeCheckError(TypeCheck(source, symbols), line, column, expectedMessage);
        }
    }
}