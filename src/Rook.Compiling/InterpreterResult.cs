using System.Collections.Generic;
using System.Linq;

namespace Rook.Compiling
{
    public class InterpreterResult
    {
        public InterpreterResult(object value)
        {
            Value = value;
            Errors = Enumerable.Empty<CompilerError>();
            Language = Language.Rook;
        }

        public InterpreterResult(Language language, params CompilerError[] errors)
           : this(language, (IEnumerable<CompilerError>)errors)
        {
        }

        public InterpreterResult(Language language, IEnumerable<CompilerError> errors)
        {
            Value = null;
            Errors = errors;
            Language = language;
        }

        public object Value { get; private set; }
        public IEnumerable<CompilerError> Errors { get; private set; }
        public Language Language { get; private set; }
    }
}