using System.Linq;
using System.Reflection;
using Rook.Compiling.Syntax;
using Rook.Core.Collections;

namespace Rook.Compiling
{
    public class CompilerResult
    {
        public CompilerResult(Assembly compiledAssembly)
        {
            CompiledAssembly = compiledAssembly;
            Errors = Enumerable.Empty<CompilerError>().ToVector();
            Language = Language.Rook;
        }

        public CompilerResult(Language language, params CompilerError[] errors)
            : this(language, errors.ToVector()) { }

        public CompilerResult(Language language, Vector<CompilerError> errors)
        {
            CompiledAssembly = null;
            Errors = errors;
            Language = language;
        }

        public Assembly CompiledAssembly { get; private set; }
        public Vector<CompilerError> Errors { get; private set; }
        public Language Language { get; private set; }
    }
}