using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class IntegerLiteral : Expression
    {
        public Position Position { get; private set; }
        public string Digits { get; private set; }
        public DataType Type { get { return NamedType.Integer; } }

        public IntegerLiteral(Position position, string digits)
        {
            Position = position;

            //TODO: We're currently just trusting that the given string contains digits
            //      and that it is a string of digits we can use (ie would fit in the 
            //      target int/long/etc type).

            Digits = digits;
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