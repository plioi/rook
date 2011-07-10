using System.Linq;
using NUnit.Framework;

namespace Rook.Compiling
{
    public abstract class CompilerTests<TCompiler> where TCompiler : Compiler
    {
        protected abstract TCompiler Compiler { get; }
        protected CompilerResult result;

        [SetUp]
        public void SetUp()
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
            if (!result.Errors.Any(x => x.Line == line && x.Column == column && x.Message == expectedMessage))
                Fail.WithErrors(result.Errors, line, column, expectedMessage);
        }

        protected object ExecuteMain()
        {
            return result.CompiledAssembly.GetType("Program").GetMethod("Main").Invoke(null, null);
        }
    }
}