using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class BooleanLiteral : Expression
    {
        public Position Position { get; private set; }
        public bool Value { get; private set; }
        public DataType Type { get { return NamedType.Boolean; } }

        public BooleanLiteral(Position position, bool value)
        {
            Position = position;
            Value = value;
        }

        public TypeChecked<Expression> WithTypes(TypeChecker visitor, Scope scope, TypeUnifier unifier)
        {
            return visitor.TypeCheck(this, scope, unifier);
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}