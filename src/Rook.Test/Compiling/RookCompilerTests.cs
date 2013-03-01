using Should;

namespace Rook.Compiling
{
    [Facts]
    public class RookCompilerTests : CompilerTests<RookCompiler>
    {
        protected override RookCompiler Compiler
        {
            get { return new RookCompiler(CompilerParameters.ForBasicEvaluation()); }
        }

        public void ShouldReportParseErrors()
        {
            Build("int Main() {$1}");
            AssertErrors(1);
            AssertError(1, 13, "Parse error.");
        }

        public void ShouldReportValidationErrors()
        {
            Build("int Main() {x}");
            AssertErrors(1);
            AssertError(1, 13, "Reference to undefined identifier: x");
        }

        public void ShouldBuildAssembliesFromSourceCode()
        {
            Build("int Main() {123}");
            AssertErrors(0);
            Execute().ShouldEqual(123);
        }
    }
}
