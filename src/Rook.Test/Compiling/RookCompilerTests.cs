using Parsley;
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
        public void ShouldBuildAssembliesFromSourceCode()
        {
            Build("int Main() 123;");
            AssertErrors(0);
            Execute().ShouldEqual(123);
        }

        [Fact]
        public void ShouldBuildAssembliesFromSyntaxTrees()
        {
            var tokens = new RookLexer().Tokenize("int Main() 123;");
            var compilationUnit = new RookGrammar().CompilationUnit.Parse(new TokenStream(tokens)).Value;
            Build(compilationUnit);
            AssertErrors(0);
            Execute().ShouldEqual(123);
        }

        private void Build(CompilationUnit compilationUnit)
        {
            UseResult(Compiler.Build(compilationUnit));
        }
    }
}
