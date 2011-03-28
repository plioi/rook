using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public sealed class Parameter : SyntaxTree, Binding
    {
        public Position Position { get; private set; }
        public DataType Type { get; private set; }
        public string Identifier { get; private set; }

        public Parameter(Position position, string identifier)
            : this (position, null, identifier) { }

        public Parameter(Position position, DataType type, string identifier)
        {
            Position = position;
            Type = type;
            Identifier = identifier;
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}