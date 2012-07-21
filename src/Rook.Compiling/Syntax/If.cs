using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class If : Expression
    {
        public Position Position { get; private set; }
        public Expression Condition { get; private set; }
        public Expression BodyWhenTrue { get; private set; }
        public Expression BodyWhenFalse { get; private set; }
        public DataType Type { get; private set; }

        public If(Position position, Expression condition, Expression bodyWhenTrue, Expression bodyWhenFalse)
            : this(position, condition, bodyWhenTrue, bodyWhenFalse, null) { }

        public If(Position position, Expression condition, Expression bodyWhenTrue, Expression bodyWhenFalse, DataType type)
        {
            Position = position;
            Condition = condition;
            BodyWhenTrue = bodyWhenTrue;
            BodyWhenFalse = bodyWhenFalse;
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