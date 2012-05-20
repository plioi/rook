using System.Linq;
using Parsley;
using Should;

namespace Rook.Compiling
{
    public abstract class CompilerTests<TCompiler> where TCompiler : Compiler
    {
        protected abstract TCompiler Compiler { get; }
        protected CompilerResult result;

        protected CompilerTests()
        {
            result = null;
        }

        protected void Build(string code)
        {
            UseResult(Compiler.Build(code));
        }

        protected void UseResult(CompilerResult resultForAssertions)
        {
            result = resultForAssertions;
        }

        protected void AssertErrors(int expectedErrorCount)
        {
            result.Errors.Count().ShouldEqual(expectedErrorCount);

            if (expectedErrorCount > 0)
                result.CompiledAssembly.ShouldBeNull();
            else
                result.CompiledAssembly.ShouldNotBeNull();
        }

        protected void AssertError(int line, int column, string expectedMessage)
        {
            var expectedPosition = new Position(line, column);
            if (!result.Errors.Any(x => x.Position == expectedPosition && x.Message == expectedMessage))
                Fail.WithErrors(result.Errors, expectedPosition, expectedMessage);
        }

        protected object Execute()
        {
            return result.CompiledAssembly.Execute();
        }
    }
}