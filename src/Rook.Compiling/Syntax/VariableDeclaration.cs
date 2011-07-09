using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class VariableDeclaration : SyntaxTree, Binding
    {
        public Position Position { get; private set; }
        public DataType Type { get; private set; }
        public string Identifier { get; private set; }
        public Expression Value { get; private set; }

        public VariableDeclaration(Position position, string identifier, Expression value)
            : this(position, null, identifier, value) { }

        public VariableDeclaration(Position position, DataType declaredType, string identifier, Expression value)
        {
            Position = position;
            Type = declaredType;
            Identifier = identifier;
            Value = value;
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}