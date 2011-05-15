using System.Text;
using NUnit.Framework;
using Parsley;
using Text = Parsley.Text;
using Rook.Compiling.Syntax;

namespace Rook.Compiling.CodeGeneration
{
    [TestFixture]
    public class CSharpTranslatorSpec
    {
        private StringBuilder expectation;

        [SetUp]
        public void SetUp()
        {
            expectation = new StringBuilder();
        }

        [Test]
        public void ShouldTranslatePrograms()
        {
            StringBuilder program = new StringBuilder()
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
            AssertTranslation(Grammar.Program, program.ToString());
        }

        [Test]
        public void ShouldTranslateFunctionsWithoutArguments()
        {
            Expect("public static bool Negatory()");
            Expect("{");
            Expect("    return false;");
            Expect("}");
            AssertTranslation(Grammar.Function, "bool Negatory() false");
        }

        [Test]
        public void ShouldTranslateFunctionsWithArguments()
        {
            Expect("public static int Sum(int a, int b, int c)");
            Expect("{");
            Expect("    return ((((a) + (b))) + (c));");
            Expect("}");
            AssertTranslation(Grammar.Function, "int Sum(int a, int b, int c) a+b+c");
        }

        [Test]
        public void ShouldTranslateVoidFunctions()
        {
            Expect("public static Rook.Core.Void PrintSum(int a, int b, int c)");
            Expect("{");
            Expect("    return (Print(((((a) + (b))) + (c))));");
            Expect("}");
            AssertTranslation(Grammar.Function, "void PrintSum(int a, int b, int c) Print(a+b+c)");
        }

        [Test]
        public void ShouldTranslateNames()
        {
            AssertTranslation("foo", Grammar.Name, "foo");
            AssertTranslation("bar", Grammar.Name, "bar");
        }

        [Test]
        public void ShouldTranslateBlocksWithOneInnerExpression()
        {
            Expect("_Block(() =>");
            Expect("{");
            Expect("    return 0;");
            Expect("}");
            Expect(")");
            AssertTranslation(Grammar.Expression, "{ 0; }");
        }

        [Test]
        public void ShouldTranslateBlocksWithMultipleInnerExpressions()
        {
            Expect("_Block(() =>");
            Expect("{");
            Expect("    _Evaluate(true);");
            Expect("    _Evaluate(((true) || (false)));");
            Expect("    return 0;");
            Expect("}");
            Expect(")");
            AssertTranslation(Grammar.Expression, "{ true; true||false; 0; }");
        }

        [Test]
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
            AssertTranslation(Grammar.Expression, "{ int a = 1; int b = 2; int c = a+b; a+b; c==3; }");
        }
        
        [Test]
        public void ShouldTranslateLambdaExpressions()
        {
            AssertTranslation("() => 0", Grammar.Expression, "fn () 0");
            AssertTranslation("() => x", Grammar.Expression, "fn () x");
            AssertTranslation("() => ((1) + (2))", Grammar.Expression, "fn () 1+2");

            AssertTranslation("(int a) => ((a) + (1))", Grammar.Expression, "fn (int a) a+1");
            AssertTranslation("(int a, int b) => ((a) + (b))", Grammar.Expression, "fn (int a, int b) a+b");
        }

        [Test]
        public void ShouldTranslateIfExpressions()
        {
            Expect("((((x) == (2))) ? (((0) + (1))) : (((1) + (2))))");
            AssertTranslation(Grammar.Expression, "if (x==2) 0+1 else 1+2");
        }

        [Test]
        public void ShouldTranslateCallExpressionsForUnaryOperators()
        {
            AssertTranslation("(-(5))", Grammar.Expression, "-5");
            AssertTranslation("(-(x))", Grammar.Expression, "-x");
            AssertTranslation("(!(true))", Grammar.Expression, "!true");
            AssertTranslation("(!(b))", Grammar.Expression, "!b");
        }

        [Test]
        public void ShouldTranslateCallExpressionsForBinaryOperators()
        {
            AssertTranslation("((1) + (2))", Grammar.Expression, "1+2");
            AssertTranslation("((true) || (false))", Grammar.Expression, "true||false");
            AssertTranslation("((x) * (y))", Grammar.Expression, "x*y");
            AssertTranslation("(((x) != null) ? ((x).Value) : (y))", Grammar.Expression, "x??y");
        }

        [Test]
        public void ShouldTranslateCallExpressionsForNamedFunctions()
        {
            AssertTranslation("(Foo())", Grammar.Expression, "Foo()");
            AssertTranslation("(Foo(true))", Grammar.Expression, "Foo(true)");
            AssertTranslation("(Foo(1, x, false))", Grammar.Expression, "Foo(1, x, false)");
        }

        [Test]
        public void ShouldTranslateBooleanLiterals()
        {
            AssertTranslation("true", Grammar.Expression, "true");
            AssertTranslation("false", Grammar.Expression, "false");
        }

        [Test]
        public void ShouldTranslateIntegerLiterals()
        {
            AssertTranslation("123", Grammar.Expression, "123");
        }

        [Test]
        public void ShouldTranslateNulls()
        {
            AssertTranslation("null", Grammar.Expression, "null");
        }

        [Test]
        public void ShouldTranslateVectorLiterals()
        {
            AssertTranslation("_Vector(0)", Grammar.Expression, "[0]");
            AssertTranslation("_Vector(0, ((1) + (2)))", Grammar.Expression, "[0, 1+2]");
            AssertTranslation("_Vector(true, ((false) || (true)), false)", Grammar.Expression, "[true, false||true, false]");
        }

        private void Expect(string line)
        {
            expectation.AppendLine(line);
        }

        private static void AssertTranslation<T>(string expectedCSharp, Parser<T> parse, string rookSource) 
            where T : SyntaxTree
        {
            Translate(parse, rookSource).ShouldEqual(expectedCSharp);
        }

        private void AssertTranslation<T>(Parser<T> parse, string rookSource) where T : SyntaxTree
        {
            Translate(parse, rookSource).TrimEnd().ShouldEqual(expectation.ToString().TrimEnd());
        }

        private static string Translate<T>(Parser<T> parse, string rookSource)
            where T : SyntaxTree
        {
            Text text = new Text(rookSource);
            CodeWriter code = new CodeWriter();
            WriteAction write = parse(text).Value.Visit(new CSharpTranslator());
            write(code);
            return code.ToString();
        }
    }
}
