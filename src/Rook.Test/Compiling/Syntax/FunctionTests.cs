﻿using System.Linq;
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
            FailsToParse("", "").WithMessage("(1, 1): type name expected");
            FailsToParse("int", "");
            FailsToParse("int foo", "").WithMessage("(1, 8): ( expected");
            FailsToParse("int foo(", "").WithMessage("(1, 9): ) expected");
            FailsToParse("int foo(x)", "");
            FailsToParse("int foo(int)", "int)").WithMessage("(1, 9): ) expected");
            FailsToParse("int foo()", "");
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
            Function node = Parse("bool foo(int x, int y, bool z) x+y>0 && z");
            node.Parameters.ElementAt(0).Type.ShouldEqual(Integer);
            node.Parameters.ElementAt(1).Type.ShouldEqual(Integer);
            node.Parameters.ElementAt(2).Type.ShouldEqual(Boolean);
            node.Body.Type.ShouldBeNull();
            node.Type.ShouldBeNull();

            Function typedNode = node.WithTypes(Environment()).Syntax;
            typedNode.Parameters.ElementAt(0).Type.ShouldEqual(Integer);
            typedNode.Parameters.ElementAt(1).Type.ShouldEqual(Integer);
            typedNode.Parameters.ElementAt(2).Type.ShouldEqual(Boolean);
            typedNode.Body.Type.ShouldEqual(Boolean);
            typedNode.Type.ShouldEqual(NamedType.Function(new[] { Integer, Integer, Boolean }, Boolean));
        }

        [Fact]
        public void FailsTypeCheckingWhenBodyExpressionFailsTypeChecking()
        {
            AssertTypeCheckError(1, 15, "Type mismatch: expected int, found bool.", "int foo() true+0");
        }

        [Fact]
        public void FailsTypeCheckingWhenParameterNamesAreNotUnique()
        {
            AssertTypeCheckError(1, 34, "Duplicate identifier: x", "int foo(int x, int y, int z, int x) true");
        }

        [Fact]
        public void FailsTypeCheckingWhenParameterNamesShadowSurroundingScope()
        {
            AssertTypeCheckError(1, 27, "Duplicate identifier: z", "int foo(int x, int y, int z) true;", z => Integer);
        }

        [Fact]
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