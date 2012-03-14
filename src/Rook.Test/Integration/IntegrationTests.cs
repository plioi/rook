using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Rook.Compiling;
using Should;
using Xunit;

namespace Rook.Integration
{
    public class IntegrationTests
    {
        [Fact] public void ArithmeticExpression() { Run("ArithmeticExpression"); }
        [Fact] public void BooleanExpression() { Run("BooleanExpression"); }
        [Fact] public void FunctionCall() { Run("FunctionCall"); }
        [Fact] public void BlockExpression() { Run("BlockExpression"); }
        [Fact] public void Closure() { Run("Closure"); }
        [Fact] public void Recursion() { Run("Recursion"); }
        [Fact] public void MutualRecursion() { Run("MutualRecursion"); }
        [Fact] public void Enumerable() { Run("Enumerable"); }
        [Fact] public void Vector() { Run("Vector"); }
        [Fact] public void Void() { Run("Void"); }
        [Fact] public void Nullable() { Run("Nullable"); }
        [Fact] public void String() { Run("String"); }

        #region Integration Test Runner

        private const string SampleFolder = "Samples";

        private static void Run(string testName)
        {
            ActualOutput(testName).ShouldEqual(ExpectedOutput(testName));
        }

        private static string ExpectedOutput(string testName)
        {
            return File.ReadAllText(Path.Combine(SampleFolder, testName + ".rook.out"));
        }

        private static string ActualOutput(string testName)
        {
            return ExecuteMain(Build(RookCode(testName))).ToString();
        }

        private static string RookCode(string testName)
        {
            return File.ReadAllText(Path.Combine(SampleFolder, testName + ".rook"));
        }

        private static Assembly Build(string rookCode)
        {
            var result = new RookCompiler(CompilerParameters.ForBasicEvaluation()).Build(rookCode);

            if (result.Errors.Any())
                Fail.WithErrors(result.Errors);

            return result.CompiledAssembly;
        }

        private static object ExecuteMain(Assembly assembly)
        {
            var stringBuilder = new StringBuilder();
            using (TextWriter writer = new StringWriter(stringBuilder))
            {
                TextWriter standardOut = Console.Out;
                Console.SetOut(writer);
                
                object result = assembly.GetType("Program").GetMethod("Main").Invoke(null, null);

                Console.SetOut(standardOut);

                string writerResult = stringBuilder.ToString();

                if ((result == null || result == Core.Void.Value) && writerResult != "")
                    return writerResult;

                return result;
            }
        }

        #endregion
    }
}