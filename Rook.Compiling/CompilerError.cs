using Parsley;

namespace Rook.Compiling
{
    public class CompilerError
    {
        public CompilerError(Position position, string message)
            : this(position.Line, position.Column, message) { }

        public CompilerError(int line, int column, string message)
        {
            Line = line;
            Column = column;
            Message = message;
        }
        
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string Message { get; private set; }
    }
}