using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class IntegerLiteral : Expression
    {
        public Position Position { get; private set; }
        public string Digits { get; private set; }
        public DataType Type { get; private set; }

        public IntegerLiteral(Position position, string digits)
            : this (position, digits, null) { }

        private IntegerLiteral(Position position, string digits, DataType type)
        {
            Position = position;
            Digits = digits;
            Type = type;
        }

        public TypeChecked<Expression> WithTypes(Scope scope)
        {
            int value;

            if (int.TryParse(Digits, out value))
                return TypeChecked<Expression>.Success(new IntegerLiteral(Position, Digits, NamedType.Integer));

            return TypeChecked<Expression>.InvalidConstantError(Position, Digits);
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}