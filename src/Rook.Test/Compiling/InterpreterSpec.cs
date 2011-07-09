using System.Linq;
using System.Text;
using NUnit.Framework;
using Rook.Compiling.Syntax;

namespace Rook.Compiling
{
    [TestFixture]
    public class InterpreterSpec
    {
        private Interpreter interpreter;

        [SetUp]
        public void SetUp()
        {
            interpreter = new Interpreter();
        }

        [Test]
        public void ShouldDetermineWhetherSourceCodeParsesCompletelyAsAnExpressionOrFunction()
        {
            const string expression = "((5 + 2) > 5) && true";
            const string function = "int Square(int x) x*x";
            const string incompleteExpression = "(5 + ";
            const string functionWithAdditionalContent = function + function;

            interpreter.CanParse(expression).ShouldBeTrue();
            interpreter.CanParse(function).ShouldBeTrue();
            interpreter.CanParse(incompleteExpression).ShouldBeFalse();
            interpreter.CanParse(functionWithAdditionalContent).ShouldBeFalse();
        }

        [Test]
        public void ShouldEvaluateSimpleExpressions()
        {
            var result = interpreter.Interpret("1");
            result.Value.ShouldEqual(1);
            result.Errors.Count().ShouldEqual(0);
        }

        [Test]
        public void ShouldFailWhenCannotParse()
        {
            var result = interpreter.Interpret("(5 + ");
            result.Value.ShouldBeNull();
            result.Errors.Count().ShouldEqual(1);
            result.Errors.First().Message.ShouldEqual("Cannot evaluate this code: must be a function or expression.");
        }

        [Test]
        public void ShouldFailWhenExpressionFailsTypeChecking()
        {
            var result = interpreter.Interpret("(5 + true)");
            result.Value.ShouldBeNull();
            result.Errors.Count().ShouldEqual(1);
            result.Errors.First().Message.ShouldEqual("Type mismatch: expected int, found bool.");
        }

        [Test]
        public void ShouldFailWhenFunctionFailsTypeChecking()
        {
            var result = interpreter.Interpret("int Square(int x) true");
            result.Value.ShouldBeNull();
            result.Errors.Count().ShouldEqual(1);
            result.Errors.First().Message.ShouldEqual("Type mismatch: expected int, found bool.");
        }

        [Test]
        public void ShouldEvaluateExpressionsAgainstPreviouslyInterpretedFunctions()
        {
            var square = interpreter.Interpret("int Square(int x) x*x");
            var cube = interpreter.Interpret("int Cube(int x) x*x*x");
            square.Value.ShouldBeInstanceOf<Function>();
            cube.Value.ShouldBeInstanceOf<Function>();

            var result = interpreter.Interpret("Square(2) + Cube(3)");
            result.Value.ShouldEqual(31);
            result.Errors.Count().ShouldEqual(0);
        }

        [Test]
        public void ShouldAllowFunctionDefinitionsToBeReplaced()
        {
            //First definitions compile but aren't defined accurately.
            var square = interpreter.Interpret("int Square(int x) x");
            var cube = interpreter.Interpret("int Cube(int x) x");
            square.Value.ShouldBeInstanceOf<Function>();
            cube.Value.ShouldBeInstanceOf<Function>();
            var result = interpreter.Interpret("Square(2) + Cube(3)");
            result.Value.ShouldEqual(5);
            result.Errors.Count().ShouldEqual(0);

            //Second definitions don't compile.  Previous definitions persist.
            square = interpreter.Interpret("int Square(int x) false");
            cube = interpreter.Interpret("int Cube(int x) true");
            square.Value.ShouldBeNull();
            cube.Value.ShouldBeNull();
            result = interpreter.Interpret("Square(2) + Cube(3)");
            result.Value.ShouldEqual(5);
            result.Errors.Count().ShouldEqual(0);

            //Third definitions compile and replace originals.
            square = interpreter.Interpret("int Square(int x) x*x");
            cube = interpreter.Interpret("int Cube(int x) x*x*x");
            square.Value.ShouldBeInstanceOf<Function>();
            cube.Value.ShouldBeInstanceOf<Function>();
            result = interpreter.Interpret("Square(2) + Cube(3)");
            result.Value.ShouldEqual(31);
            result.Errors.Count().ShouldEqual(0);
        }

        [Test]
        public void ShouldValidateFunctionsAgainstPreviouslyInterpretedFunctions()
        {
            var square = interpreter.Interpret("int Square(int x) x*x");
            var cube = interpreter.Interpret("int Cube(int x) Square(x)*x");
            square.Value.ShouldBeInstanceOf<Function>();
            cube.Value.ShouldBeInstanceOf<Function>();

            var result = interpreter.Interpret("Square(2) + Cube(3)");
            result.Value.ShouldEqual(31);
            result.Errors.Count().ShouldEqual(0);
        }

        [Test]
        public void ShouldTranslateFunctionsToTargetLanguage()
        {
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

        [Test]
        public void DisallowsCallsToMainBecauseMainIsReservedForExpressionEvaluation()
        {
            interpreter.Interpret("5");
            var translation = interpreter.Translate();
            translation.Contains("public static int Main()").ShouldBeTrue();

            var result = interpreter.Interpret("Main()");
            result.Value.ShouldBeNull();
            result.Errors.Count().ShouldEqual(1);
            result.Errors.First().Message.ShouldEqual("Reference to undefined identifier: Main");
        }

        [Test]
        public void DisallowsExplicitDefinitionOfMainFunctionBecauseMainIsReservedForExpressionEvaluation()
        {
            var result = interpreter.Interpret("int Main(int x) x*x");
            result.Value.ShouldBeNull();
            result.Errors.Count().ShouldEqual(1);
            result.Errors.First().Message.ShouldEqual("The Main function is reserved for expression evaluation, and cannot be explicitly defined.");
        }
    }
}