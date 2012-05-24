using System.Text;
using Rook.Compiling.Syntax;
using Should;
using Xunit;

namespace Rook.Compiling
{
    public class InterpreterTests
    {
        private readonly Interpreter interpreter;

        public InterpreterTests()
        {
            interpreter = new Interpreter();
        }

        [Fact]
        public void ShouldDetermineWhetherSourceCodeParsesCompletelyAsAnExpressionOrFunctionOrClass()
        {
            const string expression = "((5 + 2) > 5) && true";
            const string function = "int Square(int x) x*x";
            const string @class = "class Foo { }";
            const string incompleteExpression = "(5 + ";
            const string functionWithAdditionalContent = function + function;
            const string classWithAdditionalContent = @class + @class;

            interpreter.CanParse(expression).ShouldBeTrue();
            interpreter.CanParse(function).ShouldBeTrue();
            interpreter.CanParse(@class).ShouldBeTrue();

            interpreter.CanParse(incompleteExpression).ShouldBeFalse();
            interpreter.CanParse(functionWithAdditionalContent).ShouldBeFalse();
            interpreter.CanParse(classWithAdditionalContent).ShouldBeFalse();
        }

        [Fact]
        public void ShouldEvaluateSimpleExpressions()
        {
            var result = interpreter.Interpret("1");
            result.Value.ShouldEqual(1);
            result.Errors.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldAllowEndLineCausedByUserHittingReturn()
        {
            var result = interpreter.Interpret("1\n");
            result.Value.ShouldEqual(1);
            result.Errors.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldFailWhenCannotParse()
        {
            var result = interpreter.Interpret("(5 + ");
            result.Value.ShouldBeNull();
            result.Errors.ShouldList(error => error.Message.ShouldEqual("Cannot evaluate this code: must be a class, function or expression."));
        }

        [Fact]
        public void ShouldFailWhenExpressionFailsTypeChecking()
        {
            var result = interpreter.Interpret("(5 + true)");
            result.Value.ShouldBeNull();
            result.Errors.ShouldList(error => error.Message.ShouldEqual("Type mismatch: expected int, found bool."));
        }

        [Fact]
        public void ShouldFailWhenFunctionFailsTypeChecking()
        {
            var result = interpreter.Interpret("int Square(int x) true");
            result.Value.ShouldBeNull();
            result.Errors.ShouldList(error => error.Message.ShouldEqual("Type mismatch: expected int, found bool."));
        }

        [Fact]
        public void ShouldEvaluateExpressionsAgainstPreviouslyInterpretedClassesAndFunctions()
        {
            var foo = interpreter.Interpret("class Foo { }");
            var square = interpreter.Interpret("int Square(int x) x*x");
            var cube = interpreter.Interpret("int Cube(int x) x*x*x");
            foo.Value.ShouldBeType<Class>();
            square.Value.ShouldBeType<Function>();
            cube.Value.ShouldBeType<Function>();

            var result = interpreter.Interpret("Square(2) + Cube(3)");
            result.Value.ShouldEqual(31);
            result.Errors.ShouldBeEmpty();

            result = interpreter.Interpret("new Foo()");
            result.Value.ToString().ShouldEqual("Program+Foo");
            result.Errors.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldAllowFunctionDefinitionsToBeReplaced()
        {
            //TODO: Test coverage for classes also being replaceable,
            //      and for classes/functions to replace each other.

            //First definitions compile but aren't defined accurately.
            var square = interpreter.Interpret("int Square(int x) x");
            var cube = interpreter.Interpret("int Cube(int x) x");
            square.Value.ShouldBeType<Function>();
            cube.Value.ShouldBeType<Function>();
            var result = interpreter.Interpret("Square(2) + Cube(3)");
            result.Value.ShouldEqual(5);
            result.Errors.ShouldBeEmpty();

            //Second definitions don't compile.  Previous definitions persist.
            square = interpreter.Interpret("int Square(int x) false");
            cube = interpreter.Interpret("int Cube(int x) true");
            square.Value.ShouldBeNull();
            cube.Value.ShouldBeNull();
            result = interpreter.Interpret("Square(2) + Cube(3)");
            result.Value.ShouldEqual(5);
            result.Errors.ShouldBeEmpty();

            //Third definitions compile and replace originals.
            square = interpreter.Interpret("int Square(int x) x*x");
            cube = interpreter.Interpret("int Cube(int x) x*x*x");
            square.Value.ShouldBeType<Function>();
            cube.Value.ShouldBeType<Function>();
            result = interpreter.Interpret("Square(2) + Cube(3)");
            result.Value.ShouldEqual(31);
            result.Errors.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldValidateFunctionsAgainstPreviouslyInterpretedFunctions()
        {
            var square = interpreter.Interpret("int Square(int x) x*x");
            var cube = interpreter.Interpret("int Cube(int x) Square(x)*x");
            square.Value.ShouldBeType<Function>();
            cube.Value.ShouldBeType<Function>();

            var result = interpreter.Interpret("Square(2) + Cube(3)");
            result.Value.ShouldEqual(31);
            result.Errors.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldTranslateClassesAndFunctionsToTargetLanguage()
        {
            interpreter.Interpret("class Foo { }");
            interpreter.Interpret("int Square(int x) x*x");
            interpreter.Interpret("int Cube(int x) Square(x)*x");

            var expected = new StringBuilder()
                .AppendLine("using System;")
                .AppendLine("using System.Collections.Generic;")
                .AppendLine("using Rook.Core;")
                .AppendLine("using Rook.Core.Collections;")
                .AppendLine()
                .AppendLine("public class Program : Prelude")
                .AppendLine("{")
                .AppendLine("    public class Foo")
                .AppendLine("    {")
                .AppendLine("    }")
                .AppendLine("    public static int Square(int x)")
                .AppendLine("    {")
                .AppendLine("        return ((x) * (x));")
                .AppendLine("    }")
                .AppendLine("    public static int Cube(int x)")
                .AppendLine("    {")
                .AppendLine("        return (((Square(x))) * (x));")
                .AppendLine("    }")
                .AppendLine("}");
            interpreter.Translate().ShouldEqual(expected.ToString());

            interpreter.Interpret("Cube(3)");

            var expectedWithMainExpression = new StringBuilder()
                .AppendLine("using System;")
                .AppendLine("using System.Collections.Generic;")
                .AppendLine("using Rook.Core;")
                .AppendLine("using Rook.Core.Collections;")
                .AppendLine()
                .AppendLine("public class Program : Prelude")
                .AppendLine("{")
                .AppendLine("    public class Foo")
                .AppendLine("    {")
                .AppendLine("    }")
                .AppendLine("    public static int Square(int x)")
                .AppendLine("    {")
                .AppendLine("        return ((x) * (x));")
                .AppendLine("    }")
                .AppendLine("    public static int Cube(int x)")
                .AppendLine("    {")
                .AppendLine("        return (((Square(x))) * (x));")
                .AppendLine("    }")
                .AppendLine("    public static int Main()")
                .AppendLine("    {")
                .AppendLine("        return (Cube(3));")
                .AppendLine("    }")
                .AppendLine("}");
            interpreter.Translate().ShouldEqual(expectedWithMainExpression.ToString());
        }

        [Fact]
        public void DisallowsCallsToMainBecauseMainIsReservedForExpressionEvaluation()
        {
            interpreter.Interpret("5");
            var translation = interpreter.Translate();
            translation.Contains("public static int Main()").ShouldBeTrue();

            var result = interpreter.Interpret("Main()");
            result.Value.ShouldBeNull();
            result.Errors.ShouldList(error => error.Message.ShouldEqual("Reference to undefined identifier: Main"));
        }

        [Fact]
        public void DisallowsExplicitDefinitionOfMainClassBecauseMainIsReservedForExpressionEvaluation()
        {
            var result = interpreter.Interpret("class Main { }");
            result.Value.ShouldBeNull();
            result.Errors.ShouldList(error => error.Message.ShouldEqual("The Main function is reserved for expression evaluation, and cannot be explicitly defined."));
        }

        [Fact]
        public void DisallowsExplicitDefinitionOfMainFunctionBecauseMainIsReservedForExpressionEvaluation()
        {
            var result = interpreter.Interpret("int Main(int x) x*x");
            result.Value.ShouldBeNull();
            result.Errors.ShouldList(error => error.Message.ShouldEqual("The Main function is reserved for expression evaluation, and cannot be explicitly defined."));
        }
    }
}