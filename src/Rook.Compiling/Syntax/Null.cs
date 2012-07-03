using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class Null : Expression
    {
        public Position Position { get; private set; }
        public DataType Type { get; private set; }

        public Null(Position position) 
            : this(position, null) { }

        public Null(Position position, DataType type)
        {
            Position = position;
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