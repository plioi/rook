using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class Name : Expression
    {
        public Position Position { get; private set; }
        public string Identifier { get; private set; }
        public DataType Type { get; private set; }

        public Name(Position position, string identifier)
            : this (position, identifier, null) { }

        public Name(Position position, string identifier, DataType type)
        {
            Position = position;
            Identifier = identifier;
            Type = type;
        }

        public TypeChecked<Expression> WithTypes(TypeChecker visitor, Scope scope)
        {
            return visitor.TypeCheck(this, scope);
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}