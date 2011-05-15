using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Rook.Compiling;

namespace Rook.Integration
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test] public void ArithmeticExpression() { Run(); }
        [Test] public void BooleanExpression() { Run(); }
        [Test] public void FunctionCall() { Run(); }
        [Test] public void BlockExpression() { Run(); }
        [Test] public void Closure() { Run(); }
        [Test] public void Recursion() { Run(); }
        [Test] public void MutualRecursion() { Run(); }
        [Test] public void Enumerable() { Run(); }
        [Test] public void Vector() { Run(); }
        [Test] public void Void() { Run(); }
        [Test] public void Nullable() { Run(); }

        #region Integration Test Runner

        private const string SampleFolder = "Samples";

        private static void Run()
        {
            string testName = new StackTrace().GetFrame(1).GetMethod().Name;

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