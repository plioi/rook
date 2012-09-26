using System.Text;
using Parsley;
using Rook.Compiling.Syntax;
using Should;

namespace Rook.Compiling.CodeGeneration
{
    [Facts]
    public class CSharpTranslatorTests
    {
        private readonly RookGrammar rookGrammar;
        private readonly StringBuilder expectation;
        
        public CSharpTranslatorTests()
        {
            rookGrammar = new RookGrammar();
            expectation = new StringBuilder();
        }

        public void ShouldTranslateCompilationUnits()
        {
            var compilationUnit = new StringBuilder()
                .AppendLine("class Foo { }")
                .AppendLine("class Bar { int I() 0; bool B() false; }")
                .AppendLine("int Life() 14")
                .AppendLine("int Universe() 14")
                .AppendLine("int Everything() 14")
                .AppendLine("int Main() Life()+Universe()+Everything()");

            Expect("using System;");
            Expect("using System.Collections.Generic;");
            Expect("using Rook.Core;");
            Expect("using Rook.Core.Collections;");
            Expect("");
            Expect("public class __program__ : Prelude");
            Expect("{");
            Expect("    public class Foo");
            Expect("    {");
            Expect("    }");
            Expect("    public class Bar");
            Expect("    {");
            Expect("        public int I()");
            Expect("        {");
            Expect("            return 0;");
            Expect("        }");
            Expect("        public bool B()");
            Expect("        {");
            Expect("            return false;");
            Expect("        }");
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
            AssertTranslation(rookGrammar.CompilationUnit, compilationUnit.ToString());
        }

        public void ShouldTranslateClasses()
        {
            Expect("public class Foo");
            Expect("{");
            Expect("    public int I()");
            Expect("    {");
            Expect("        return 0;");
            Expect("    }");
            Expect("    public bool B()");
            Expect("    {");
            Expect("        return false;");
            Expect("    }");
            Expect("}");
            AssertTranslation(rookGrammar.Class, "class Foo { int I() 0; bool B() false; }");
        }

        public void ShouldTranslateFunctionsWithoutArguments()
        {
            Expect("public static bool Negatory()");
            Expect("{");
            Expect("    return false;");
            Expect("}");
            AssertTranslation(rookGrammar.Function, "bool Negatory() false");
        }

        public void ShouldTranslateFunctionsWithArguments()
        {
            Expect("public static int Sum(int a, int b, int c)");
            Expect("{");
            Expect("    return ((((a) + (b))) + (c));");
            Expect("}");
            AssertTranslation(rookGrammar.Function, "int Sum(int a, int b, int c) a+b+c");
        }

        public void ShouldTranslateVoidFunctions()
        {
            Expect("public static Rook.Core.Void PrintSum(int a, int b, int c)");
            Expect("{");
            Expect("    return (Print(((((a) + (b))) + (c))));");
            Expect("}");
            AssertTranslation(rookGrammar.Function, "void PrintSum(int a, int b, int c) Print(a+b+c)");
        }

        public void ShouldTranslateNames()
        {
            AssertTranslation("foo", rookGrammar.Name, "foo");
            AssertTranslation("bar", rookGrammar.Name, "bar");
        }

        public void ShouldTranslateBlocksWithOneInnerExpression()
        {
            Expect("__block__(() =>");
            Expect("{");
            Expect("    return 0;");
            Expect("}");
            Expect(")");
            AssertTranslation(rookGrammar.Expression, "{ 0; }");
        }

        public void ShouldTranslateBlocksWithMultipleInnerExpressions()
        {
            Expect("__block__(() =>");
            Expect("{");
            Expect("    __evaluate__(true);");
            Expect("    __evaluate__(((true) || (false)));");
            Expect("    return 0;");
            Expect("}");
            Expect(")");
            AssertTranslation(rookGrammar.Expression, "{ true; true||false; 0; }");
        }

        public void ShouldTranslateBlocksWithLocalVariableDeclarations()
        {
            Expect("__block__(() =>");
            Expect("{");
            Expect("    int a = 1;");
            Expect("    int b = 2;");
            Expect("    var c = ((a) + (b));");
            Expect("    __evaluate__(((a) + (b)));");
            Expect("    return ((c) == (3));");
            Expect("}");
            Expect(")");
            AssertTranslation(rookGrammar.Expression, "{ int a = 1; int b = 2; c = a+b; a+b; c==3; }");
        }
        
        public void ShouldTranslateLambdaExpressions()
        {
            AssertTranslation("() => 0", rookGrammar.Expression, "fn () 0");
            AssertTranslation("() => x", rookGrammar.Expression, "fn () x");
            AssertTranslation("() => ((1) + (2))", rookGrammar.Expression, "fn () 1+2");

            AssertTranslation("(int a) => ((a) + (1))", rookGrammar.Expression, "fn (int a) a+1");
            AssertTranslation("(int a, int b) => ((a) + (b))", rookGrammar.Expression, "fn (int a, int b) a+b");
        }

        public void ShouldTranslateIfExpressions()
        {
            Expect("((((x) == (2))) ? (((0) + (1))) : (((1) + (2))))");
            AssertTranslation(rookGrammar.Expression, "if (x==2) 0+1 else 1+2");
        }

        public void ShouldTranslateCallExpressionsForUnaryOperators()
        {
            AssertTranslation("(-(5))", rookGrammar.Expression, "-5");
            AssertTranslation("(-(x))", rookGrammar.Expression, "-x");
            AssertTranslation("(!(true))", rookGrammar.Expression, "!true");
            AssertTranslation("(!(b))", rookGrammar.Expression, "!b");
        }

        public void ShouldTranslateCallExpressionsForBinaryOperators()
        {
            AssertTranslation("((1) + (2))", rookGrammar.Expression, "1+2");
            AssertTranslation("((true) || (false))", rookGrammar.Expression, "true||false");
            AssertTranslation("((x) * (y))", rookGrammar.Expression, "x*y");
            AssertTranslation("(((x) != null) ? ((x).Value) : (y))", rookGrammar.Expression, "x??y");
        }

        public void ShouldTranslateCallExpressionsForNamedFunctions()
        {
            AssertTranslation("(Foo())", rookGrammar.Expression, "Foo()");
            AssertTranslation("(Foo(true))", rookGrammar.Expression, "Foo(true)");
            AssertTranslation("(Foo(1, x, false))", rookGrammar.Expression, "Foo(1, x, false)");
        }

        public void ShouldTranslateMethodInvocation()
        {
            AssertTranslation("x.Method()", rookGrammar.Expression, "x.Method()");
            AssertTranslation("x.Method(true)", rookGrammar.Expression, "x.Method(true)");
            AssertTranslation("x.Method(1, z, false)", rookGrammar.Expression, "x.Method(1, z, false)");
        }

        public void ShouldTranslateConstructorCalls()
        {
            AssertTranslation("(new Foo())", rookGrammar.Expression, "new Foo()");
        }

        public void ShouldTranslateBooleanLiterals()
        {
            AssertTranslation("true", rookGrammar.Expression, "true");
            AssertTranslation("false", rookGrammar.Expression, "false");
        }

        public void ShouldTranslateIntegerLiterals()
        {
            AssertTranslation("123", rookGrammar.Expression, "123");
        }

        public void ShouldTranslateStringLiterals()
        {
            AssertTranslation("\"\"", rookGrammar.Expression, "\"\"");
            AssertTranslation("\"abc \\\" \\\\ \\n \\r \\t \\u263a def\"", rookGrammar.Expression, "\"abc \\\" \\\\ \\n \\r \\t \\u263a def\"");
        }

        public void ShouldTranslateNulls()
        {
            AssertTranslation("null", rookGrammar.Expression, "null");
        }

        public void ShouldTranslateVectorLiterals()
        {
            AssertTranslation("__vector__(0)", rookGrammar.Expression, "[0]");
            AssertTranslation("__vector__(0, ((1) + (2)))", rookGrammar.Expression, "[0, 1+2]");
            AssertTranslation("__vector__(true, ((false) || (true)), false)", rookGrammar.Expression, "[true, false||true, false]");
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
