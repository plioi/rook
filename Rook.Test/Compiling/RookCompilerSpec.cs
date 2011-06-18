using NUnit.Framework;
using Rook.Compiling.Syntax;

namespace Rook.Compiling
{
    [TestFixture]
    public class RookCompilerSpec : CompilerSpec<RookCompiler>
    {
        protected override RookCompiler Compiler
        {
            get { return new RookCompiler(CompilerParameters.ForBasicEvaluation()); }
        }

        [Test]
        public void ShouldReportParseErrors()
        {
            Build("int Main() $1;");
            AssertErrors(1);
            AssertError(1, 12, "(, [, {, Boolean, fn, Identifier, if, Integer or null expected");
        }

        [Test]
        public void ShouldReportValidationErrors()
        {
            Build("int Main() x;");
            AssertErrors(1);
            AssertError(1, 12, "Reference to undefined identifier: x");
        }

        [Test]
        public void ShouldBuildProgramsFromSourceCode()
        {
            Build("int Main() 123;");
            AssertErrors(0);
            ExecuteMain().ShouldEqual(123);
        }

        [Test]
        public void ShouldBuildProgramsFromSyntaxTrees()
        {
            Program program = Grammar.Program(new RookLexer("int Main() 123;")).Value;
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
