using Parsley;

namespace Rook.Compiling.Syntax
{
    public class Class : SyntaxTree
    {
        public Position Position { get; private set; }

        public Name Name { get; private set; }

        public Class(Position position, Name name)
        {
            Position = position;
            Name = name;
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}