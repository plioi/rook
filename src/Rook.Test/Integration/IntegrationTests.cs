using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Rook.Compiling;
using Should;

namespace Rook.Integration
{
    public class IntegrationTests
    {
        public void ArithmeticExpression() { Run("ArithmeticExpression"); }
        public void BooleanExpression() { Run("BooleanExpression"); }
        public void FunctionCall() { Run("FunctionCall"); }
        public void MethodInvocation() { Run("MethodInvocation"); }
        public void BlockExpression() { Run("BlockExpression"); }
        public void Closure() { Run("Closure"); }
        public void Recursion() { Run("Recursion"); }
        public void MutualRecursion() { Run("MutualRecursion"); }
        public void Enumerable() { Run("Enumerable"); }
        public void Vector() { Run("Vector"); }
        public void Void() { Run("Void"); }
        public void Nullable() { Run("Nullable"); }
        public void String() { Run("String"); }
        public void Classes() { Run("Classes"); }
        public void IndexerOverloading() { Run("IndexerOverloading"); }

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
            return Execute(Build(RookCode(testName))).ToString();
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

        private static object Execute(Assembly assembly)
        {
            var stringBuilder = new StringBuilder();
            using (TextWriter writer = new StringWriter(stringBuilder))
            using (new VirtualConsole(writer))
            {
                var result = assembly.Execute();

                var writerResult = stringBuilder.ToString();

                if ((result == null || result == Core.Void.Value) && writerResult != "")
                    return writerResult;

                return result;
            }
        }

        private class VirtualConsole : IDisposable
        {
            private readonly TextWriter standardOut;

            public VirtualConsole(TextWriter writer)
            {
                standardOut = Console.Out;
                Console.SetOut(writer);
            }

            public void Dispose()
            {
                Console.SetOut(standardOut);
            }
        }

        #endregion
    }
}