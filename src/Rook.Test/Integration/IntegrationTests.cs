using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Rook.Compiling;
using Should;

namespace Rook.Integration
{
    public class IntegrationTests
    {
        public void ArithmeticExpression() { Run(); }
        public void BooleanExpression() { Run(); }
        public void FunctionCall() { Run(); }
        public void MethodInvocation() { Run(); }
        public void BlockExpression() { Run(); }
        public void Closure() { Run(); }
        public void Recursion() { Run(); }
        public void MutualRecursion() { Run(); }
        public void Enumerable() { Run(); }
        public void Vector() { Run(); }
        public void Void() { Run(); }
        public void Nullable() { Run(); }
        public void String() { Run(); }
        public void Classes() { Run(); }
        public void IndexerOverloading() { Run(); }

        private const string SampleFolder = "Samples";

        private static void Run([CallerMemberName] string testName = null)
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
    }
}