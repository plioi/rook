using System.Linq;
using Rook.Compiling.Syntax;
using Rook.Core.Collections;

namespace Rook.Compiling
{
    public class InterpreterResult
    {
        public InterpreterResult(object value)
        {
            Value = value;
            Errors = Enumerable.Empty<CompilerError>().ToVector();
            Language = Language.Rook;
        }

        public InterpreterResult(Language language, params CompilerError[] errors)
            : this(language, errors.ToVector()) { }

        public InterpreterResult(Language language, Vector<CompilerError> errors)
        {
            Value = null;
            Errors = errors;
            Language = language;
        }

        public object Value { get; private set; }
        public Vector<CompilerError> Errors { get; private set; }
        public Language Language { get; private set; }
    }
}