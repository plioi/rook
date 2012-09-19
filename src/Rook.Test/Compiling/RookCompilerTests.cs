using System.Collections.Generic;
using Parsley;
using Rook.Compiling.Syntax;
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
            Build("int Main() $1;");
            AssertErrors(1);
            AssertError(1, 12, "Parse error.");
        }

        public void ShouldReportValidationErrors()
        {
            Build("int Main() x;");
            AssertErrors(1);
            AssertError(1, 12, "Reference to undefined identifier: x");
        }

        public void ShouldBuildAssembliesFromSourceCode()
        {
            Build("int Main() 123;");
            AssertErrors(0);
            Execute().ShouldEqual(123);
        }

        public void ShouldBuildAssembliesFromSyntaxTrees()
        {
            var tokens = "int Main() 123;".Tokenize();
            var compilationUnit = new RookGrammar().CompilationUnit.Parse(tokens).Value;
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
