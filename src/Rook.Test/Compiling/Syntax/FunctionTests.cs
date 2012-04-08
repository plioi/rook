using Parsley;
using Rook.Compiling.Types;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class FunctionTests : SyntaxTreeTests<Function>
    {
        protected override Parser<Function> Parser { get { return RookGrammar.Function; } }

        [Fact]
        public void DemandsCompleteFunctionDefinition()
        {
            FailsToParse("").AtEndOfInput().WithMessage("(1, 1): type name expected");
            FailsToParse("int").AtEndOfInput();
            FailsToParse("int foo").AtEndOfInput().WithMessage("(1, 8): ( expected");
            FailsToParse("int foo(").AtEndOfInput().WithMessage("(1, 9): ) expected");
            FailsToParse("int foo(x)").AtEndOfInput();
            FailsToParse("int foo(int)").LeavingUnparsedTokens("int", ")").WithMessage("(1, 9): ) expected");
            FailsToParse("int foo()").AtEndOfInput();
            Parses("int foo() 1").IntoTree("int foo() 1");
        }

        [Fact]
        public void ParsesReturnTypes()
        {
            Parses("int foo() 1").IntoTree("int foo() 1");
            Parses("bool foo() 1").IntoTree("bool foo() 1");
        }

        [Fact]
        public void ParsesNames()
        {
            Parses("int foo() 1").IntoTree("int foo() 1");
            Parses("int bar() 1").IntoTree("int bar() 1");
        }

        [Fact]
        public void ParsesParameterLists()
        {
            Parses("int foo() 1").IntoTree("int foo() 1");
            Parses("int foo(int x) 1").IntoTree("int foo(int x) 1");
            Parses("int foo(int x, bool y) 1").IntoTree("int foo(int x, bool y) 1");
            Parses("int foo(int x, bool y, int z) 1").IntoTree("int foo(int x, bool y, int z) 1");
        }

        [Fact]
        public void AllowsParametersToOmitExplicitTypeDeclaration()
        {
            Parses("int foo(x) 1").IntoTree("int foo(x) 1");
            Parses("int foo(x, bool y) 1").IntoTree("int foo(x, bool y) 1");
            Parses("int foo(int x, y, z) 1").IntoTree("int foo(int x, y, z) 1");
        }

        [Fact]
        public void ParsesBodyExpression()
        {
            Parses("int foo() 1").IntoTree("int foo() 1");
            Parses("int foo() false").IntoTree("int foo() false");
        }

        [Fact]
        public void HasATypeIncludingInputTypesAndReturnType()
        {
            Type("int foo() 1").ShouldEqual(NamedType.Function(Integer));
            Type("bool foo(int x) false").ShouldEqual(NamedType.Function(new[] {Integer}, Boolean));
            Type("int foo(int x, bool y) 1").ShouldEqual(NamedType.Function(new[] {Integer, Boolean}, Integer));
        }

        [Fact]
        public void EvaluatesBodyExpressionTypesInANewScopeIncludingParameters()
        {
            Type("int foo(int x) x").ShouldEqual(NamedType.Function(new[] {Integer}, Integer));
            Type("bool foo(int x, int y, bool b) x==y || b").ShouldEqual(
                NamedType.Function(new[] {Integer, Integer, Boolean}, Boolean));
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var node = Parse("bool foo(int x, int y, bool z) x+y>0 && z");
            node.Parameters.ShouldHaveTypes(Integer, Integer, Boolean);
            node.Body.Type.ShouldBeNull();
            node.Type.ShouldBeNull();

            var typedNode = node.WithTypes(Environment()).Syntax;
            typedNode.Parameters.ShouldHaveTypes(Integer, Integer, Boolean);
            typedNode.Body.Type.ShouldEqual(Boolean);
            typedNode.Type.ShouldEqual(NamedType.Function(new[] { Integer, Integer, Boolean }, Boolean));
        }

        [Fact]
        public void FailsTypeCheckingWhenBodyExpressionFailsTypeChecking()
        {
            TypeChecking("int foo() true+0").ShouldFail("Type mismatch: expected int, found bool.", 1, 15);
        }

        [Fact]
        public void FailsTypeCheckingWhenParameterNamesAreNotUnique()
        {
            TypeChecking("int foo(int x, int y, int z, int x) true").ShouldFail("Duplicate identifier: x", 1, 34);
        }

        [Fact]
        public void FailsTypeCheckingWhenParameterNamesShadowSurroundingScope()
        {
            TypeChecking("int foo(int x, int y, int z) true;", z => Integer).ShouldFail("Duplicate identifier: z", 1, 27);
        }

        [Fact]
        public void FailsTypeCheckingWhenDeclaredReturnTypeDoesNotMatchBodyExpressionType()
        {
            TypeChecking("int foo (int x) false").ShouldFail("Type mismatch: expected int, found bool.", 1, 5);
        }

        private DataType Type(string source, params TypeMapping[] symbols)
        {
            return Parse(source).WithTypes(Environment(symbols)).Syntax.Type;
        }

        private TypeChecked<Function> TypeChecking(string source, params TypeMapping[] symbols)
        {
            return Parse(source).WithTypes(Environment(symbols));
        }
    }
}