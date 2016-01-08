using System.IO;
using System.Linq;
using System.Reflection;
using Fixie.Internal;
using Rook.Compiling;
using Should;

namespace Rook.IntegrationTest
{
    public class IntegrationTests
    {
        public void ProgramShouldHaveExpectedOutput(string programName)
        {
            var expectedOutput = File.ReadAllText(programName + ".rook.out");
            var actualOutput = Execute(Build(File.ReadAllText(programName + ".rook"))).ToString();

            actualOutput.ShouldEqual(expectedOutput);
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
            using (var console = new RedirectedConsole())
            {
                var result = assembly.Execute();

                var writerResult = console.Output;

                if ((result == null || result == Core.Void.Value) && writerResult != "")
                    return writerResult;

                return result;
            }
        }
    }
}