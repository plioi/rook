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
        }

        public InterpreterResult(params CompilerError[] errors)
           : this((IEnumerable<CompilerError>)errors)
        {
        }

        public InterpreterResult(IEnumerable<CompilerError> errors)
        {
            Value = null;
            Errors = errors;
        }

        public object Value { get; private set; }
        public IEnumerable<CompilerError> Errors { get; private set; }
    }
}