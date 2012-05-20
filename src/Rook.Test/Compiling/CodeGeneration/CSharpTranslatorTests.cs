using System.Text;
using Parsley;
using Rook.Compiling.Syntax;
using Should;
using Xunit;

namespace Rook.Compiling.CodeGeneration
{
    public class CSharpTranslatorTests
    {
        private readonly RookGrammar rookGrammar;
        private readonly StringBuilder expectation;
        
        public CSharpTranslatorTests()
        {
            rookGrammar = new RookGrammar();
            expectation = new StringBuilder();
        }

        [Fact]
        public void ShouldTranslatePrograms()
        {
            var program = new StringBuilder()
                .AppendLine("class Foo")
                .AppendLine("class Bar")
                .AppendLine("int Life() 14")
                .AppendLine("int Universe() 14")
                .AppendLine("int Everything() 14")
                .AppendLine("int Main() Life()+Universe()+Everything()");

            Expect("using System;");
            Expect("using System.Collections.Generic;");
            Expect("using Rook.Core;");
            Expect("using Rook.Core.Collections;");
            Expect("");
            Expect("public class Program : Prelude");
            Expect("{");
            Expect("    public class Foo");
            Expect("    {");
            Expect("    }");
            Expect("    public class Bar");
            Expect("    {");
            Expect("    }");
            Expect("    public static int Life()");
            Expect("    {");
            Expect("        return 14;");
            Expect("    }");
            Expect("    public static int Universe()");
            Expect("    {");
            Expect("        return 14;");
            Expect("    }");
            Expect("    public static int Everything()");
            Expect("    {");
            Expect("        return 14;");
            Expect("    }");
            Expect("    public static int Main()");
            Expect("    {");
            Expect("        return (((((Life())) + ((Universe())))) + ((Everything())));");
            Expect("    }");
            Expect("}");
            AssertTranslation(rookGrammar.Program, program.ToString());
        }

        [Fact]
        public void ShouldTranslateClasses()
        {
            Expect("public class Foo");
            Expect("{");
            Expect("}");
            AssertTranslation(rookGrammar.Class, "class Foo");
        }

        [Fact]
        public void ShouldTranslateFunctionsWithoutArguments()
        {
            Expect("public static bool Negatory()");
            Expect("{");
            Expect("    return false;");
            Expect("}");
            AssertTranslation(rookGrammar.Function, "bool Negatory() false");
        }

        [Fact]
        public void ShouldTranslateFunctionsWithArguments()
        {
            Expect("public static int Sum(int a, int b, int c)");
            Expect("{");
            Expect("    return ((((a) + (b))) + (c));");
            Expect("}");
            AssertTranslation(rookGrammar.Function, "int Sum(int a, int b, int c) a+b+c");
        }

        [Fact]
        public void ShouldTranslateVoidFunctions()
        {
            Expect("public static Rook.Core.Void PrintSum(int a, int b, int c)");
            Expect("{");
            Expect("    return (Print(((((a) + (b))) + (c))));");
            Expect("}");
            AssertTranslation(rookGrammar.Function, "void PrintSum(int a, int b, int c) Print(a+b+c)");
        }

        [Fact]
        public void ShouldTranslateNames()
        {
            AssertTranslation("foo", rookGrammar.Name, "foo");
            AssertTranslation("bar", rookGrammar.Name, "bar");
        }

        [Fact]
        public void ShouldTranslateBlocksWithOneInnerExpression()
        {
            Expect("_Block(() =>");
            Expect("{");
            Expect("    return 0;");
            Expect("}");
            Expect(")");
            AssertTranslation(rookGrammar.Expression, "{ 0; }");
        }

        [Fact]
        public void ShouldTranslateBlocksWithMultipleInnerExpressions()
        {
            Expect("_Block(() =>");
            Expect("{");
            Expect("    _Evaluate(true);");
            Expect("    _Evaluate(((true) || (false)));");
            Expect("    return 0;");
            Expect("}");
            Expect(")");
            AssertTranslation(rookGrammar.Expression, "{ true; true||false; 0; }");
        }

        [Fact]
        public void ShouldTranslateBlocksWithLocalVariableDeclarations()
        {
            Expect("_Block(() =>");
            Expect("{");
            Expect("    int a = 1;");
            Expect("    int b = 2;");
            Expect("    int c = ((a) + (b));");
            Expect("    _Evaluate(((a) + (b)));");
            Expect("    return ((c) == (3));");
            Expect("}");
            Expect(")");
            AssertTranslation(rookGrammar.Expression, "{ int a = 1; int b = 2; int c = a+b; a+b; c==3; }");
        }
        
        [Fact]
        public void ShouldTranslateLambdaExpressions()
        {
            AssertTranslation("() => 0", rookGrammar.Expression, "fn () 0");
            AssertTranslation("() => x", rookGrammar.Expression, "fn () x");
            AssertTranslation("() => ((1) + (2))", rookGrammar.Expression, "fn () 1+2");

            AssertTranslation("(int a) => ((a) + (1))", rookGrammar.Expression, "fn (int a) a+1");
            AssertTranslation("(int a, int b) => ((a) + (b))", rookGrammar.Expression, "fn (int a, int b) a+b");
        }

        [Fact]
        public void ShouldTranslateIfExpressions()
        {
            Expect("((((x) == (2))) ? (((0) + (1))) : (((1) + (2))))");
            AssertTranslation(rookGrammar.Expression, "if (x==2) 0+1 else 1+2");
        }

        [Fact]
        public void ShouldTranslateCallExpressionsForUnaryOperators()
        {
            AssertTranslation("(-(5))", rookGrammar.Expression, "-5");
            AssertTranslation("(-(x))", rookGrammar.Expression, "-x");
            AssertTranslation("(!(true))", rookGrammar.Expression, "!true");
            AssertTranslation("(!(b))", rookGrammar.Expression, "!b");
        }

        [Fact]
        public void ShouldTranslateCallExpressionsForBinaryOperators()
        {
            AssertTranslation("((1) + (2))", rookGrammar.Expression, "1+2");
            AssertTranslation("((true) || (false))", rookGrammar.Expression, "true||false");
            AssertTranslation("((x) * (y))", rookGrammar.Expression, "x*y");
            AssertTranslation("(((x) != null) ? ((x).Value) : (y))", rookGrammar.Expression, "x??y");
        }

        [Fact]
        public void ShouldTranslateCallExpressionsForNamedFunctions()
        {
            AssertTranslation("(Foo())", rookGrammar.Expression, "Foo()");
            AssertTranslation("(Foo(true))", rookGrammar.Expression, "Foo(true)");
            AssertTranslation("(Foo(1, x, false))", rookGrammar.Expression, "Foo(1, x, false)");
        }

        [Fact]
        public void ShouldTranslateConstructorCalls()
        {
            AssertTranslation("(new Foo())", rookGrammar.Expression, "new Foo()");
        }

        [Fact]
        public void ShouldTranslateBooleanLiterals()
        {
            AssertTranslation("true", rookGrammar.Expression, "true");
            AssertTranslation("false", rookGrammar.Expression, "false");
        }

        [Fact]
        public void ShouldTranslateIntegerLiterals()
        {
            AssertTranslation("123", rookGrammar.Expression, "123");
        }

        [Fact]
        public void ShouldTranslateStringLiterals()
        {
            AssertTranslation("\"\"", rookGrammar.Expression, "\"\"");
            AssertTranslation("\"abc \\\" \\\\ \\n \\r \\t \\u263a def\"", rookGrammar.Expression, "\"abc \\\" \\\\ \\n \\r \\t \\u263a def\"");
        }

        [Fact]
        public void ShouldTranslateNulls()
        {
            AssertTranslation("null", rookGrammar.Expression, "null");
        }

        [Fact]
        public void ShouldTranslateVectorLiterals()
        {
            AssertTranslation("_Vector(0)", rookGrammar.Expression, "[0]");
            AssertTranslation("_Vector(0, ((1) + (2)))", rookGrammar.Expression, "[0, 1+2]");
            AssertTranslation("_Vector(true, ((false) || (true)), false)", rookGrammar.Expression, "[true, false||true, false]");
        }

        private void Expect(string line)
        {
            expectation.AppendLine(line);
        }

        private static void AssertTranslation<T>(string expectedCSharp, Parser<T> parser, string rookSource) 
            where T : SyntaxTree
        {
            Translate(parser, rookSource).ShouldEqual(expectedCSharp);
        }

        private void AssertTranslation<T>(Parser<T> parser, string rookSource) where T : SyntaxTree
        {
            Translate(parser, rookSource).TrimEnd().ShouldEqual(expectation.ToString().TrimEnd());
        }

        private static string Translate<T>(Parser<T> parser, string rookSource)
            where T : SyntaxTree
        {
            var code = new CodeWriter();
            WriteAction write = parser.Parses(rookSource).Value.Visit(new CSharpTranslator());
            write(code);
            return code.ToString();
        }
    }
}
