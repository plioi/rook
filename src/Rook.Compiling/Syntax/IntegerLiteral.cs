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
            : this(position, digits, UnknownType.Instance) { }

        public IntegerLiteral(Position position, string digits, DataType type)
        {
            Position = position;
            Digits = digits;
            Type = type;
        }

        public Expression WithTypes(TypeChecker visitor, Scope scope)
        {
            return visitor.TypeCheck(this, scope);
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}