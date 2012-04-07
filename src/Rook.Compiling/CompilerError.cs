using Parsley;

namespace Rook.Compiling
{
    public class CompilerError
    {
        public CompilerError(Position position, string message)
        {
            Message = message;
            Position = position;
        }

        public Position Position { get; private set; }
        public string Message { get; private set; }
    }
}