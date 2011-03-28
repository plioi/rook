using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public sealed class BooleanLiteral : Expression
    {
        public Position Position { get; private set; }
        public bool Value { get; private set; }
        public DataType Type { get { return NamedType.Boolean; } }

        public BooleanLiteral(Position position, bool value)
        {
            Position = position;
            Value = value;
        }

        public TypeChecked<Expression> WithTypes(Environment environment)
        {
            return TypeChecked<Expression>.Success(this);
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}