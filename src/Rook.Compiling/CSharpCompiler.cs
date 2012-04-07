using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CSharp;
using Parsley;

namespace Rook.Compiling
{
    public class CSharpCompiler : Compiler
    {
        private readonly CompilerParameters parameters;

        public CSharpCompiler(CompilerParameters parameters)
        {
            this.parameters = parameters;
        }

        public CompilerResult Build(string code)
        {
            var csharpParameters = new System.CodeDom.Compiler.CompilerParameters
            {
                GenerateExecutable = parameters.GenerateExecutable,
                GenerateInMemory = parameters.GenerateInMemory,
                IncludeDebugInformation = parameters.IncludeDebugInformation
            };

            foreach (var assembly in parameters.References)
                csharpParameters.ReferencedAssemblies.Add(assembly.Location);

            var options = new Dictionary<string, string> {{"CompilerVersion", "v4.0"}};
            var provider = new CSharpCodeProvider(options);
            var results = provider.CompileAssemblyFromSource(csharpParameters, code);

            if (results.Errors.HasErrors)
                return new CompilerResult(MapErrors(results.Errors));

            return new CompilerResult(results.CompiledAssembly);
        }

        private static IEnumerable<CompilerError> MapErrors(CompilerErrorCollection errors)
        {
            foreach (System.CodeDom.Compiler.CompilerError error in errors)
                yield return new CompilerError(new Position(error.Line, error.Column), error.ErrorText);
        }
    }
}