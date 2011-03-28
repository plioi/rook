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

            Assert.IsTrue(interpreter.CanParse(expression));
            Assert.IsTrue(interpreter.CanParse(function));
            Assert.IsFalse(interpreter.CanParse(incompleteExpression));
            Assert.IsFalse(interpreter.CanParse(functionWithAdditionalContent));
        }

        [Test]
        public void ShouldEvaluateSimpleExpressions()
        {
            var result = interpreter.Interpret("1");
            Assert.AreEqual(1, result.Value);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [Test]
        public void ShouldFailWhenCannotParse()
        {
            var result = interpreter.Interpret("(5 + ");
            Assert.IsNull(result.Value);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("Cannot evaluate this code: must be a function or expression.", result.Errors.First().Message);
        }

        [Test]
        public void ShouldFailWhenExpressionFailsTypeChecking()
        {
            var result = interpreter.Interpret("(5 + true)");
            Assert.IsNull(result.Value);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("Type mismatch: expected int, found bool.", result.Errors.First().Message);
        }

        [Test]
        public void ShouldFailWhenFunctionFailsTypeChecking()
        {
            var result = interpreter.Interpret("int Square(int x) true");
            Assert.IsNull(result.Value);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("Type mismatch: expected int, found bool.", result.Errors.First().Message);
        }

        [Test]
        public void ShouldEvaluateExpressionsAgainstPreviouslyInterpretedFunctions()
        {
            var square = interpreter.Interpret("int Square(int x) x*x");
            var cube = interpreter.Interpret("int Cube(int x) x*x*x");
            Assert.IsTrue(square.Value is Function);
            Assert.IsTrue(cube.Value is Function);

            var result = interpreter.Interpret("Square(2) + Cube(3)");
            Assert.AreEqual(31, result.Value);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [Test]
        public void ShouldAllowFunctionDefinitionsToBeReplaced()
        {
            //First definitions compile but aren't defined accurately.
            var square = interpreter.Interpret("int Square(int x) x");
            var cube = interpreter.Interpret("int Cube(int x) x");
            Assert.IsTrue(square.Value is Function);
            Assert.IsTrue(cube.Value is Function);
            var result = interpreter.Interpret("Square(2) + Cube(3)");
            Assert.AreEqual(5, result.Value);
            Assert.AreEqual(0, result.Errors.Count());

            //Second definitions don't compile.  Previous definitions persist.
            square = interpreter.Interpret("int Square(int x) false");
            cube = interpreter.Interpret("int Cube(int x) true");
            Assert.IsNull(square.Value);
            Assert.IsNull(cube.Value);
            result = interpreter.Interpret("Square(2) + Cube(3)");
            Assert.AreEqual(5, result.Value);
            Assert.AreEqual(0, result.Errors.Count());

            //Third definitions compile and replace originals.
            square = interpreter.Interpret("int Square(int x) x*x");
            cube = interpreter.Interpret("int Cube(int x) x*x*x");
            Assert.IsTrue(square.Value is Function);
            Assert.IsTrue(cube.Value is Function);
            result = interpreter.Interpret("Square(2) + Cube(3)");
            Assert.AreEqual(31, result.Value);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [Test]
        public void ShouldValidateFunctionsAgainstPreviouslyInterpretedFunctions()
        {
            var square = interpreter.Interpret("int Square(int x) x*x");
            var cube = interpreter.Interpret("int Cube(int x) Square(x)*x");
            Assert.IsTrue(square.Value is Function);
            Assert.IsTrue(cube.Value is Function);

            var result = interpreter.Interpret("Square(2) + Cube(3)");
            Assert.AreEqual(31, result.Value);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [Test]
        public void ShouldTranslateFunctionsToTargetLanguage()
        {
            interpreter.Interpret("int Square(int x) x*x");
            interpreter.Interpret("int Cube(int x) Square(x)*x");

            StringBuilder expected = new StringBuilder()
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
            Assert.AreEqual(expected.ToString(), interpreter.Translate());

            interpreter.Interpret("Cube(3)");

            StringBuilder expectedWithMainExpression = new StringBuilder()
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
            Assert.AreEqual(expectedWithMainExpression.ToString(), interpreter.Translate());
        }

        [Test]
        public void DisallowsCallsToMainBecauseMainIsReservedForExpressionEvaluation()
        {
            interpreter.Interpret("5");
            var translation = interpreter.Translate();
            Assert.IsTrue(translation.Contains("public static int Main()"));

            var result = interpreter.Interpret("Main()");
            Assert.IsNull(result.Value);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("Reference to undefined identifier: Main", result.Errors.First().Message);
        }

        [Test]
        public void DisallowsExplicitDefinitionOfMainFunctionBecauseMainIsReservedForExpressionEvaluation()
        {
            var result = interpreter.Interpret("int Main(int x) x*x");
            Assert.IsNull(result.Value);
            Assert.AreEqual(1, result.Errors.Count());
            Assert.AreEqual("The Main function is reserved for expression evaluation, and cannot be explicitly defined.", result.Errors.First().Message);
        }
    }
}