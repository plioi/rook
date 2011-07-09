using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rook.Compiling
{
    public class CompilerResult
    {
        public CompilerResult(Assembly compiledAssembly)
        {
            CompiledAssembly = compiledAssembly;
            Errors = Enumerable.Empty<CompilerError>();
        }

        public CompilerResult(params CompilerError[] errors)
           : this((IEnumerable<CompilerError>)errors)
        {
        }

        public CompilerResult(IEnumerable<CompilerError> errors)
        {
            CompiledAssembly = null;
            Errors = errors;
        }

        public Assembly CompiledAssembly { get; private set; }
        public IEnumerable<CompilerError> Errors { get; private set; }
    }
}