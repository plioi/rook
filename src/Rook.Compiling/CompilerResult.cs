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
            Language = Language.Rook;
        }

        public CompilerResult(Language language, params CompilerError[] errors)
           : this(language, (IEnumerable<CompilerError>)errors)
        {
        }

        public CompilerResult(Language language, IEnumerable<CompilerError> errors)
        {
            CompiledAssembly = null;
            Errors = errors;
            Language = language;
        }

        public Assembly CompiledAssembly { get; private set; }
        public IEnumerable<CompilerError> Errors { get; private set; }
        public Language Language { get; private set; }
    }
}