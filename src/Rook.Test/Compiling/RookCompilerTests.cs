using Rook.Compiling.Syntax;
using Should;
using Xunit;

namespace Rook.Compiling
{
    public class RookCompilerTests : CompilerTests<RookCompiler>
    {
        protected override RookCompiler Compiler
        {
            get { return new RookCompiler(CompilerParameters.ForBasicEvaluation()); }
        }

        [Fact]
        public void ShouldReportParseErrors()
        {
            Build("int Main() $1;");
            AssertErrors(1);
            AssertError(1, 12, "Parse error.");
        }

        [Fact]
        public void ShouldReportValidationErrors()
        {
            Build("int Main() x;");
            AssertErrors(1);
            AssertError(1, 12, "Reference to undefined identifier: x");
        }

        [Fact]
        public void ShouldBuildProgramsFromSourceCode()
        {
            Build("int Main() 123;");
            AssertErrors(0);
            ExecuteMain().ShouldEqual(123);
        }

        [Fact]
        public void ShouldBuildProgramsFromSyntaxTrees()
        {
            Program program = new RookGrammar().Program.Parse(new RookLexer("int Main() 123;")).Value;
            Build(program);
            AssertErrors(0);
            ExecuteMain().ShouldEqual(123);
        }

        private void Build(Program program)
        {
            UseResult(Compiler.Build(program));
        }
    }
}
