using Parsley;

namespace Rook.Compiling.Syntax
{
    public class Token
    {
        public Position Position { get; private set; }
        private readonly string token;

        public Token(Position position, string token)
        {
            Position = position;
            this.token = token;
        }

        public override string ToString()
        {
            return token;
        }
    }
}